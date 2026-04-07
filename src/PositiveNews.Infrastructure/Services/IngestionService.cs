using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
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

        // Build TopicLookup once for this entire cycle
        TopicLookup? topicLookup = null;
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var topics = await context.Topics.AsNoTracking().ToListAsync(cancellationToken);
            topicLookup = TopicLookup.Build(topics);
            _logger.LogInformation("Topic lookup built with {Count} topics.", topics.Count);
        }

        // Fetch the list of active sources
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

            await ProcessSourceAsync(source, topicLookup!, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Waiting {Delay} before next source...", DelayBetweenSources);
                await Task.Delay(DelayBetweenSources, cancellationToken);
            }
        }

        _logger.LogInformation("=== Ingestion cycle completed. ===");
    }

    private async Task ProcessSourceAsync(Source source, TopicLookup topicLookup, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing source: {SourceName} ({FeedUrl})", source.Name, source.FeedUrl);

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var run = new IngestionRun
        {
            SourceId = source.Id,
            StartedAt = DateTime.UtcNow,
            Status = IngestionStatus.Running
        };
        context.IngestionRuns.Add(run);
        await context.SaveChangesAsync(cancellationToken);

        int newArticleCount = 0;
        var url = source.FeedUrl!;

        try
        {
            var doc = await _feedReader.ReadFeedAsync(url, cancellationToken);

            var dtoItems = _feedProcessor.ProcessFeed(url, doc, topicLookup, out int invalidCount);

            if (dtoItems.Count == 0)
            {
                _logger.LogWarning("Source {SourceName} returned zero feed items.", source.Name);
                run.Status = IngestionStatus.Partial;
                run.ErrorMessage = "Feed returned zero items. The feed URL may be unavailable or empty.";
                run.FinishedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
                return;
            }

            foreach (var item in dtoItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (await IsAlreadyExists(item, context, cancellationToken))
                    {
                        _logger.LogDebug("Skipping duplicate: {Title}", item.Title);
                        continue;
                    }

                    await SaveArticleWithTopicsAsync(source, item, context, cancellationToken);

                    newArticleCount++;
                    _logger.LogInformation("Ingested new article: {Title}", item.Title);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error processing DTO for article: {ExternalId}", item.ExternalId);
                }
            }
            run.Status = IngestionStatus.Success;
            run.ItemsFetched = newArticleCount;
            run.FinishedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Source {SourceName}: ingested {NewCount} new articles out of {TotalCount} feed items. {InvalidCount} rejected.",
                source.Name, newArticleCount, dtoItems.Count + invalidCount, invalidCount);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Ingestion for {SourceName} was cancelled.", source.Name);
            run.Status = IngestionStatus.Partial;
            run.ErrorMessage = "Operation was cancelled.";
            run.ItemsFetched = newArticleCount;
            run.FinishedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(CancellationToken.None);
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

    private static async Task<bool> IsAlreadyExists(RssFeedItemDto dto, AppDbContext context,
                                                    CancellationToken cancellationToken)
    {
        bool alreadyExists = false;

        if (!string.IsNullOrWhiteSpace(dto.ExternalId))
        {
            alreadyExists = await context.ArticlesMetadata
                .AnyAsync(a => a.ExternalId == dto.ExternalId, cancellationToken);
        }

        if (!alreadyExists && !string.IsNullOrWhiteSpace(dto.Link))
        {
            alreadyExists = await context.ArticlesMetadata
                .AnyAsync(a => a.Url == dto.Link, cancellationToken);
        }

        return alreadyExists;
    }

    private static async Task SaveArticleWithTopicsAsync(Source source, RssFeedItemDto dto,
                                                         AppDbContext context, CancellationToken cancellationToken)
    {
        var articleMeta = new ArticleMetadata
        {
            SourceId = source.Id,
            ExternalId = dto.ExternalId,
            Title = dto.Title,
            Author = dto.Author,
            Url = dto.Link,
            ImageTag = dto.ImageTag,
            PublishedAt = dto.PublishedDate,
            IngestedAt = DateTime.UtcNow,
            LanguageCode = source.DefaultLanguageCode,
            RegionCode = "Global",
            IsActive = true,
            SummaryShort = dto.Description
        };

        var articleContent = new ArticleContent
        {
            ContentRaw = dto.ContentRaw,
        };

        articleMeta.Content = articleContent;

        context.ArticlesMetadata.Add(articleMeta);

        // Save to get the generated Id
        await context.SaveChangesAsync(cancellationToken);

        // Add topic associations
        if (dto.Topics != null && dto.Topics.Any())
        {
            var topics = await context.Topics
                .Where(t => dto.Topics.Contains(t.Name))
                .ToListAsync(cancellationToken);

            foreach (var topic in topics)
            {
                context.ArticleTopics.Add(new ArticleTopic
                {
                    ArticleId = articleMeta.Id,
                    TopicId = topic.Id
                });
            }
        }
    }
}