using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Interfaces;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;

namespace PositiveNews.Infrastructure.Services;

public class IngestionService : IIngestionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IFeedReader _feedReader;
    private readonly IFeedProcessor _feedProcessor;
    private readonly ILogger<IngestionService> _logger;

    private static readonly TimeSpan DelayBetweenSources = TimeSpan.FromSeconds(2);

    public IngestionService(
        IServiceScopeFactory scopeFactory,
        IFeedReader feedReader,
        IFeedProcessor feedProcessor,
        ILogger<IngestionService> logger)
    {
        _scopeFactory = scopeFactory;
        _feedReader = feedReader;
        _feedProcessor = feedProcessor;
        _logger = logger;
    }

    public async Task RunIngestionCycleAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== Ingestion cycle started. ===");

        TopicLookup topicLookup;
        List<Source> activeSources;

        // Scope 1: Fetch foundational data
        using (var scope = _scopeFactory.CreateScope())
        {
            var queries = scope.ServiceProvider.GetRequiredService<IIngestionQueries>();
            topicLookup = await queries.GetTopicLookupAsync(cancellationToken);
            activeSources = await queries.GetActiveSourcesAsync(cancellationToken);
        }

        _logger.LogInformation("Found {Count} active sources. Topic lookup ready.", activeSources.Count);

        foreach (var source in activeSources)
        {
            if (cancellationToken.IsCancellationRequested) break;

            await ProcessSourceAsync(source, topicLookup, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(DelayBetweenSources, cancellationToken);
            }
        }

        _logger.LogInformation("=== Ingestion cycle completed. ===");
    }

    private async Task ProcessSourceAsync(Source source, TopicLookup topicLookup, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing source: {SourceName} ({FeedUrl})", source.Name, source.FeedUrl);

        // Scope 2: Isolate the DbContext lifespan per source being processed
        using var scope = _scopeFactory.CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IIngestionQueries>();
        var commands = scope.ServiceProvider.GetRequiredService<IIngestionCommands>();

        var run = await commands.StartRunAsync(source.Id, cancellationToken);
        int newArticleCount = 0;
        var url = source.FeedUrl!;

        try
        {
            var doc = await _feedReader.ReadFeedAsync(url, cancellationToken);
            var dtoItems = _feedProcessor.ProcessFeed(url, doc, topicLookup, out int invalidCount);
            int skipCount = invalidCount;

            if (dtoItems.Count == 0)
            {
                _logger.LogWarning("Source {SourceName} returned zero feed items.", source.Name);
                await commands.CompleteRunAsync(run, IngestionStatus.Partial, 0, "Feed returned zero items.", cancellationToken);
                return;
            }

            foreach (var item in dtoItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (await queries.ArticleExistsAsync(item.ExternalId, item.Link, item.Title, cancellationToken))
                    {
                        _logger.LogDebug("Skipping duplicate: {Title}", item.Title);
                        skipCount++;
                        continue;
                    }

                    await commands.SaveArticleWithTopicsAsync(source, item, cancellationToken);
                    newArticleCount++;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error processing DTO for article: {ExternalId}", item.ExternalId);
                }
            }

            await commands.CompleteRunAsync(run, IngestionStatus.Success, newArticleCount, null, cancellationToken);
            _logger.LogInformation("Source {SourceName}: ingested {NewCount} new articles. {skipCount} rejected.", source.Name, newArticleCount, skipCount);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Ingestion for {SourceName} was cancelled.", source.Name);
            await commands.CompleteRunAsync(run, IngestionStatus.Partial, newArticleCount, "Operation was cancelled.", CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting source {SourceName}.", source.Name);
            await commands.CompleteRunAsync(run, IngestionStatus.Failed, newArticleCount, ex.Message, CancellationToken.None);
        }
    }
}