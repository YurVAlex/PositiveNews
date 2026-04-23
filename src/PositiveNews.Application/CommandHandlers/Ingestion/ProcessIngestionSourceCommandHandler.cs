using MediatR;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Application.Queries.Ingestion;
using PositiveNews.Application.Services.Ingestion;
using PositiveNews.Domain.Entities;
using System.Diagnostics;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

public sealed class ProcessIngestionSourceCommandHandler(
    IIngestionRunRepository ingestionRunRepository,
    IIngestionUnitOfWork ingestionUnitOfWork,
    IArticleDeduplicator articleDeduplicator,
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

        var run = IngestionRun.Start(source.Id);
        ingestionRunRepository.Add(run);
        await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);

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
                run.PartialComplete(0, "Feed returned zero items. The feed URL may be unavailable or empty.");
                await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);
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

            newArticleCount = await mediator.Send(
                new PersistIngestedArticlesCommand(source.Id, source.DefaultLanguageCode, toPersist),
                cancellationToken);

            run.Complete(newArticleCount);
            await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);

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
            run.PartialComplete(newArticleCount, "Operation was cancelled.");
            await ingestionUnitOfWork.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ingesting source {SourceName}.", source.Name);
            run.Fail(ex.Message, newArticleCount);
            await ingestionUnitOfWork.SaveChangesAsync(CancellationToken.None);
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

}
