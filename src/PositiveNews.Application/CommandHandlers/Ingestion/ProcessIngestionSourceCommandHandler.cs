using MediatR;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Common;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using PositiveNews.Application.Queries.Ingestion;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;
using PositiveNews.Domain.Exceptions;
using System.Diagnostics;
using System.Xml;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

/// <summary>
/// Orchestrates a single RSS poll: audit run, fetch feed, process items, dedupe, persist new articles, and finalize run status.
/// </summary>
/// <param name="ingestionRunRepository">Records ingestion run lifecycle rows.</param>
/// <param name="ingestionUnitOfWork">Commits ingestion-specific persistence.</param>
/// <param name="articleDeduplicator">Skips duplicates against DB and within-batch collisions.</param>
/// <param name="feedReader">Downloads RSS XML.</param>
/// <param name="feedProcessor">Parses and enriches feed items.</param>
/// <param name="mediator">Dispatches nested queries/commands for keys and persistence.</param>
/// <param name="logger">Structured logging for progress and failures.</param>
public sealed class ProcessIngestionSourceCommandHandler(
    IIngestionRunRepository ingestionRunRepository,
    IIngestionUnitOfWork ingestionUnitOfWork,
    IArticleDeduplicator articleDeduplicator,
    IFeedReader feedReader,
    IFeedProcessor feedProcessor,
    IMediator mediator,
    ILogger<ProcessIngestionSourceCommandHandler> logger)
    : IRequestHandler<ProcessIngestionSourceCommand, Result<int>>
{
    /// <summary>
    /// Runs the full pipeline for one source and returns how many new articles were saved (or failure details).
    /// </summary>
    /// <param name="request">Source snapshot plus shared lookup and settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of newly persisted articles or an application error.</returns>
    public async Task<Result<int>> Handle(ProcessIngestionSourceCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var source = request.Source;
        logger.LogInformation("Processing source: {SourceName} ({FeedUrl})", source.Name, source.FeedUrl);

        var run = IngestionRun.Start(source.Id);
        ingestionRunRepository.Add(run);
        await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);

        var newArticleCount = 0;

        try
        {
            var doc = await feedReader.ReadFeedAsync(source.FeedUrl, cancellationToken);
            var processingResult = feedProcessor.ProcessFeed(
                doc, request.TopicLookup, request.IngestionSettings, source, cancellationToken);
            var dtoItems = processingResult.Items;
            var invalidCount = processingResult.InvalidCount;

            var skipCount = invalidCount;

            if (dtoItems.Count == 0)
            {
                logger.LogWarning("Source {SourceName} returned zero feed items.", source.Name);
                run.PartialComplete(0, "Feed returned zero items. The feed URL may be unavailable or empty.");
                await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(0);
            }

            var existingKeys = await mediator.Send(
                new FindExistingArticleKeysQuery(
                    dtoItems.Select(d => d.ExternalId).ToList(),
                    dtoItems.Select(d => d.Link).ToList(),
                    dtoItems.Select(d => d.Title).ToList()),
                cancellationToken);

            var toPersist = new List<RssFeedItemDto>();
            var pendingExternalIds = new HashSet<string>(StringComparer.Ordinal);
            var pendingUrls = new HashSet<string>(StringComparer.Ordinal);
            var pendingTitles = new HashSet<string>(StringComparer.Ordinal);

            foreach (var item in dtoItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (articleDeduplicator.MatchesExisting(existingKeys, item))
                {
                    logger.LogDebug("Skipping duplicate: {Title}", item.Title);
                    skipCount++;
                    continue;
                }

                if (articleDeduplicator.ConflictsWithPending(item, pendingExternalIds, pendingUrls, pendingTitles))
                {
                    logger.LogDebug("Skipping duplicate within feed: {Title}", item.Title);
                    skipCount++;
                    continue;
                }

                if (!string.IsNullOrEmpty(item.ExternalId))
                    pendingExternalIds.Add(item.ExternalId);
                pendingUrls.Add(item.Link);
                pendingTitles.Add(item.Title);
                toPersist.Add(item);
            }

            var persistResult = await mediator.Send(
                new PersistIngestedArticlesCommand(source.Id, source.DefaultLanguageCode, request.TopicLookup, toPersist),
                cancellationToken);

            if (persistResult.IsFailure)
            {
                run.Fail(persistResult.Error.Message, newArticleCount);
                await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);
                return Result<int>.Failure(persistResult.Error);
            }

            newArticleCount = persistResult.Value;
            run.Complete(newArticleCount);
            await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Source {SourceName}: ingested {NewCount} new articles out of {TotalCount} feed items. {SkipCount} rejected.",
                source.Name,
                newArticleCount,
                dtoItems.Count + invalidCount,
                skipCount);

            return Result<int>.Success(newArticleCount);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain invariant violation while ingesting source {SourceName}.", source.Name);
            run.Fail(ex.Message, newArticleCount);
            await ingestionUnitOfWork.SaveChangesAsync(CancellationToken.None);
            return Result<int>.Failure(
                new Error(
                    ErrorCodes.Ingestion.DomainInvariantViolation,
                    $"Domain invariant violation for source '{source.Name}': {ex.Message}",
                    ErrorType.Conflict));
        }
        catch (OperationCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Ingestion for {SourceName} was cancelled.", source.Name);
                run.PartialComplete(newArticleCount, "Operation was cancelled.");
                await ingestionUnitOfWork.SaveChangesAsync(CancellationToken.None);
                throw;
            }

            logger.LogWarning(
                ex,
                "Feed request for {SourceName} timed out or was interrupted. Skipping source and continuing.",
                source.Name);
            run.PartialComplete(newArticleCount, "Feed request timed out or was interrupted.");
            await ingestionUnitOfWork.SaveChangesAsync(CancellationToken.None);
            return Result<int>.Success(newArticleCount);
        }
        catch (Exception ex) when (ex is HttpRequestException or XmlException or IOException)
        {
            logger.LogWarning(
                ex,
                "Feed request for {SourceName} failed. Skipping source and continuing.",
                source.Name);
            await TryPartialCompleteAndSaveAsync(run, newArticleCount, $"Feed request failed: {ex.Message}");
            return Result<int>.Success(newArticleCount);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error while ingesting source {SourceName}. Skipping source and continuing.",
                source.Name);
            await TryPartialCompleteAndSaveAsync(run, newArticleCount, $"Unexpected error: {ex.Message}");
            return Result<int>.Success(newArticleCount);
        }
        finally
        {
            await TryFinalizeOrphanedRunAsync(run, newArticleCount);

            stopwatch.Stop();
            logger.LogInformation(
                "Processing source {SourceName} finished in {ElapsedMs} ms.",
                source.Name,
                stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task TryPartialCompleteAndSaveAsync(IngestionRun run, int itemsFetched, string reason)
    {
        if (run.Status != IngestionStatus.Running)
            return;

        run.PartialComplete(itemsFetched, reason);
        try
        {
            await ingestionUnitOfWork.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist partial ingestion run status.");
        }
    }

    private async Task TryFinalizeOrphanedRunAsync(IngestionRun run, int itemsFetched)
    {
        try
        {
            if (run.Status != IngestionStatus.Running)
                return;

            run.Fail("Ingestion ended without completing.", itemsFetched);
            await ingestionUnitOfWork.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to finalize ingestion run.");
        }
    }

}
