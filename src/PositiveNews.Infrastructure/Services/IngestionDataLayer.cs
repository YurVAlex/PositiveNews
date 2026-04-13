using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Services;

public class IngestionDataLayer : IIngestionQueries, IIngestionCommands
{
    private readonly AppDbContext _context;

    public IngestionDataLayer(AppDbContext context)
    {
        _context = context;
    }

    // --- QUERIES ---

    public async Task<TopicLookup> GetTopicLookupAsync(CancellationToken cancellationToken = default)
    {
        var topics = await _context.Topics.AsNoTracking().ToListAsync(cancellationToken);
        return TopicLookup.Build(topics);
    }

    public async Task<List<Source>> GetActiveSourcesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sources
            .Where(s => s.IsActive && s.FeedUrl != null)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ArticleExistsAsync(string? externalId, string? url, string title, CancellationToken cancellationToken = default)
    {
        return await _context.ArticlesMetadata.AnyAsync(a =>
            (!string.IsNullOrWhiteSpace(externalId) && a.ExternalId == externalId) ||
            (!string.IsNullOrWhiteSpace(url) && a.Url == url) ||
            a.Title == title, cancellationToken);
    }

    // --- COMMANDS ---

    public async Task<IngestionRun> StartRunAsync(int sourceId, CancellationToken cancellationToken = default)
    {
        var run = new IngestionRun
        {
            SourceId = sourceId,
            StartedAt = DateTime.UtcNow,
            Status = IngestionStatus.Running
        };

        _context.IngestionRuns.Add(run);
        await _context.SaveChangesAsync(cancellationToken);
        return run;
    }

    public async Task CompleteRunAsync(IngestionRun run, IngestionStatus status, int itemsFetched, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        run.Status = status;
        run.ItemsFetched = itemsFetched;
        run.FinishedAt = DateTime.UtcNow;
        run.ErrorMessage = errorMessage?.Length > 4000 ? errorMessage[..4000] : errorMessage;

        _context.IngestionRuns.Update(run);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveArticleWithTopicsAsync(Source source, RssFeedItemDto dto, CancellationToken cancellationToken = default)
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
            SummaryShort = dto.Description,
            PositivityScore = dto.PositivityScore,
            AnalyzedAt = dto.PositivityScore.HasValue ? DateTime.UtcNow : null,
            Content = new ArticleContent
            {
                ContentRaw = dto.ContentRaw,
                ContentClean = dto.ContentClean
            }
        };

        _context.ArticlesMetadata.Add(articleMeta);

        if (dto.Topics != null && dto.Topics.Any())
        {
            var topics = await _context.Topics
                .Where(t => dto.Topics.Contains(t.Name))
                .ToListAsync(cancellationToken);

            foreach (var topic in topics)
            {
                _context.ArticleTopics.Add(new ArticleTopic
                {
                    Article = articleMeta, // Link via navigation property
                    TopicId = topic.Id
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}