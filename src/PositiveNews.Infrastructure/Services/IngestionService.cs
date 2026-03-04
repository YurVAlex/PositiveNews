using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Interfaces;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Iterates over all active sources, fetches their RSS feeds, 
/// deduplicates against existing articles, and persists new ones.
/// </summary>
public class IngestionService : IIngestionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRssFeedReader _feedReader;
    private readonly ILogger<IngestionService> _logger;

    /// <summary>
    /// Polite delay between processing individual sources to avoid hammering external servers.
    /// </summary>
    private static readonly TimeSpan DelayBetweenSources = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Small delay between persisting individual articles within a single source batch.
    /// </summary>
    private static readonly TimeSpan DelayBetweenArticles = TimeSpan.FromSeconds(5);

    public IngestionService(
        IServiceScopeFactory scopeFactory,
        IRssFeedReader feedReader,
        ILogger<IngestionService> logger)
    {
        _scopeFactory = scopeFactory;
        _feedReader = feedReader;
        _logger = logger;
    }

    public async Task RunIngestionCycleAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== Ingestion cycle started. ===");

        // Fetch the list of active sources.
        List<Source> activeSources;
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            activeSources = await context.Sources
                .Where(s => s.IsActive && s.FeedUrl != null)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        _logger.LogInformation("Found {Count} active sources with feed URLs.", activeSources.Count);

        foreach (var source in activeSources)
        {
            if (cancellationToken.IsCancellationRequested) break;

            await ProcessSourceAsync(source, cancellationToken);

            // Polite delay between sources.
            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Waiting {Delay} before next source...", DelayBetweenSources);
                await Task.Delay(DelayBetweenSources, cancellationToken);
            }
        }

        _logger.LogInformation("=== Ingestion cycle completed. ===");
    }

    private async Task ProcessSourceAsync(Source source, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing source: {SourceName} ({FeedUrl})", source.Name, source.FeedUrl);

        // Create a fresh scope (and therefore a fresh DbContext) for each source.
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create an IngestionRun record.
        var run = new IngestionRun
        {
            SourceId = source.Id,
            StartedAt = DateTime.UtcNow,
            Status = IngestionStatus.Running
        };
        context.IngestionRuns.Add(run);
        await context.SaveChangesAsync(cancellationToken);

        int newArticleCount = 0;

        try
        {
            var feedItems = await _feedReader.ReadFeedAsync(source.FeedUrl!, cancellationToken);

            if (feedItems.Count == 0)
            {
                _logger.LogWarning("Source {SourceName} returned zero feed items.", source.Name);
                run.Status = IngestionStatus.Partial;
                run.ErrorMessage = "Feed returned zero items. The feed URL may be unavailable or empty.";
                run.FinishedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
                return;
            }

            foreach (var item in feedItems)
            {
                if (cancellationToken.IsCancellationRequested) break;

                // Deduplication: Check if this article already exists for this source.
                bool alreadyExists = false;

                if (!string.IsNullOrWhiteSpace(item.ExternalId))
                {
                    alreadyExists = await context.ArticlesMetadata
                        .AnyAsync(a => a.SourceId == source.Id && a.ExternalId == item.ExternalId,
                                  cancellationToken);
                }

                if (!alreadyExists && !string.IsNullOrWhiteSpace(item.Link))
                {
                    alreadyExists = await context.ArticlesMetadata
                        .AnyAsync(a => a.SourceId == source.Id && a.Url == item.Link,
                                  cancellationToken);
                }

                if (alreadyExists)
                {
                    _logger.LogDebug("Skipping duplicate: {Title}", item.Title);
                    continue;
                }

                // Create the ArticleMetadata entry.
                var articleMeta = new ArticleMetadata
                {
                    SourceId = source.Id,
                    ExternalId = item.ExternalId,
                    Title = item.Title.Length > 500 ? item.Title[..500] : item.Title,
                    Author = item.Author,
                    Url = item.Link,
                    ImageUrl = item.ImageUrl,
                    PublishedAt = item.PublishedDate ?? DateTime.UtcNow,
                    IngestedAt = DateTime.UtcNow,
                    LanguageCode = source.DefaultLanguageCode,
                    RegionCode = "Global",
                    IsActive = true
                };

                context.ArticlesMetadata.Add(articleMeta);
                await context.SaveChangesAsync(cancellationToken);

                // Create the companion ArticleContent row (initially empty, to be filled by a future scraper).
                var articleContent = new ArticleContent
                {
                    Id = articleMeta.Id,
                    ContentRaw = item.Description // Store RSS description as raw content for now.
                };
                context.ArticlesContent.Add(articleContent);
                await context.SaveChangesAsync(cancellationToken);

                newArticleCount++;
                _logger.LogInformation("Ingested new article: [{Id}] {Title}", articleMeta.Id, articleMeta.Title);

                // Small polite delay between articles.
                await Task.Delay(DelayBetweenArticles, cancellationToken);
            }

            run.Status = IngestionStatus.Success;
            run.ItemsFetched = newArticleCount;
            run.FinishedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Source {SourceName}: ingested {NewCount} new articles out of {TotalCount} feed items.",
                source.Name, newArticleCount, feedItems.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Ingestion for {SourceName} was cancelled.", source.Name);
            run.Status = IngestionStatus.Partial;
            run.ErrorMessage = "Operation was cancelled.";
            run.ItemsFetched = newArticleCount;
            run.FinishedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(CancellationToken.None); // Save even on cancellation.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting source {SourceName}.", source.Name);
            run.Status = IngestionStatus.Failed;
            run.ErrorMessage = ex.Message.Length > 4000 ? ex.Message[..4000] : ex.Message;
            run.ItemsFetched = newArticleCount;
            run.FinishedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}