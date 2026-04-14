using MediatR;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Application.Queries.Ingestion;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;
using System.Diagnostics;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

public sealed class ProcessIngestionSourceCommandHandler(
    IIngestionDbContext db,
    IFeedReader feedReader,
    IFeedProcessor feedProcessor,
    IMediator mediator,
    ILogger<ProcessIngestionSourceCommandHandler> logger)
    : IRequestHandler<ProcessIngestionSourceCommand>
{
    public async Task Handle(ProcessIngestionSourceCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var source = request.Source;
        logger.LogInformation("Processing source: {SourceName} ({FeedUrl})", source.Name, source.FeedUrl);

        var run = new IngestionRun
        {
            SourceId = source.Id,
            StartedAt = DateTime.UtcNow,
            Status = IngestionStatus.Running
        };
        db.IngestionRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var newArticleCount = 0;
        var url = source.FeedUrl;

        try
        {
            var doc = await feedReader.ReadFeedAsync(url, cancellationToken);
            var processingResult = feedProcessor.ProcessFeed(url, doc, request.TopicLookup, cancellationToken);
            var dtoItems = processingResult.Items;
            var invalidCount = processingResult.InvalidCount;

            var skipCount = invalidCount;

            if (dtoItems.Count == 0)
            {
                logger.LogWarning("Source {SourceName} returned zero feed items.", source.Name);
                run.Status = IngestionStatus.Partial;
                run.ErrorMessage = "Feed returned zero items. The feed URL may be unavailable or empty.";
                run.FinishedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
                return;
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

                if (existingKeys.Matches(item))
                {
                    logger.LogDebug("Skipping duplicate: {Title}", item.Title);
                    skipCount++;
                    continue;
                }

                if (ConflictsWithPendingBatch(item, pendingExternalIds, pendingUrls, pendingTitles))
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

            newArticleCount = await mediator.Send(
                new PersistIngestedArticlesCommand(source.Id, source.DefaultLanguageCode, toPersist),
                cancellationToken);

            run.Status = IngestionStatus.Success;
            run.ItemsFetched = newArticleCount;
            run.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Source {SourceName}: ingested {NewCount} new articles out of {TotalCount} feed items. {SkipCount} rejected.",
                source.Name,
                newArticleCount,
                dtoItems.Count + invalidCount,
                skipCount);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Ingestion for {SourceName} was cancelled.", source.Name);
            run.Status = IngestionStatus.Partial;
            run.ErrorMessage = "Operation was cancelled.";
            run.ItemsFetched = newArticleCount;
            run.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ingesting source {SourceName}.", source.Name);
            run.Status = IngestionStatus.Failed;
            run.ErrorMessage = ex.Message.Length > 4000 ? ex.Message[..4000] : ex.Message;
            run.ItemsFetched = newArticleCount;
            run.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation(
                "Processing source {SourceName} finished in {ElapsedMs} ms.",
                source.Name,
                stopwatch.ElapsedMilliseconds);
        }
    }

    private static bool ConflictsWithPendingBatch(
        RssFeedItemDto item,
        HashSet<string> pendingExternalIds,
        HashSet<string> pendingUrls,
        HashSet<string> pendingTitles)
    {
        if (!string.IsNullOrEmpty(item.ExternalId) && pendingExternalIds.Contains(item.ExternalId))
            return true;
        if (pendingUrls.Contains(item.Link))
            return true;
        return pendingTitles.Contains(item.Title);
    }
}
