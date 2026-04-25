using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Ingestion;
using PositiveNews.Application.Mapping;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

internal sealed class ArticleReadRepository(AppDbContext db) : IArticleReadRepository
{
    public async Task<ArticleFeedPageResult> GetFeedPageAsync(ArticleFeedFilter filter, CancellationToken ct)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var topic = string.IsNullOrWhiteSpace(filter.Topic) ? null : filter.Topic.Trim();

        var query = db.ArticlesMetadata
            .Where(a => a.IsActive)
            .AsNoTracking();

        if (topic != null)
        {
            query = query
                .OrderByDescending(a => a.ArticleTopics.Any(at => at.Topic!.Name == topic))
                .ThenByDescending(a => a.PublishedAt);
        }
        else
        {
            query = query.OrderByDescending(a => a.PublishedAt);
        }

        var totalArticles = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

        var articles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToArticleFeedItemDto()
            .ToListAsync(ct);

        return new ArticleFeedPageResult
        {
            Articles = articles,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            SelectedTopic = topic
        };
    }

    public async Task<ArticleDetailDto?> GetDetailAsync(long id, CancellationToken ct)
    {
        var article = await db.ArticlesMetadata
            .Include(a => a.Source)
            .Include(a => a.Content)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, ct);

        if (article == null)
            return null;

        return article.ToArticleDetailDto();
    }

    public async Task<ExistingArticleKeys> FindExistingKeysAsync(
        IReadOnlyCollection<string?> externalIds,
        IReadOnlyCollection<string> urls,
        IReadOnlyCollection<string> titles,
        CancellationToken ct)
    {
        var extSet = new HashSet<string>(StringComparer.Ordinal);
        var urlSet = new HashSet<string>(StringComparer.Ordinal);
        var titleSet = new HashSet<string>(StringComparer.Ordinal);

        var extDistinct = externalIds
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct()
            .ToList();

        foreach (var chunk in extDistinct.Chunk(IngestionPipelineConstants.SqlInClauseChunkSize))
        {
            var chunkArr = chunk.ToArray();
            var batch = await db.ArticlesMetadata.AsNoTracking()
                .Where(a => a.ExternalId != null && chunkArr.Contains(a.ExternalId))
                .Select(a => a.ExternalId!)
                .ToListAsync(ct);
            foreach (var key in batch)
                extSet.Add(key);
        }

        var distinctUrls = urls.Distinct().ToList();
        foreach (var chunk in distinctUrls.Chunk(IngestionPipelineConstants.SqlInClauseChunkSize))
        {
            var chunkArr = chunk.ToArray();
            var batch = await db.ArticlesMetadata.AsNoTracking()
                .Where(a => chunkArr.Contains(a.Url))
                .Select(a => a.Url)
                .ToListAsync(ct);
            foreach (var key in batch)
                urlSet.Add(key);
        }

        var distinctTitles = titles.Distinct().ToList();
        foreach (var chunk in distinctTitles.Chunk(IngestionPipelineConstants.SqlInClauseChunkSize))
        {
            var chunkArr = chunk.ToArray();
            var batch = await db.ArticlesMetadata.AsNoTracking()
                .Where(a => chunkArr.Contains(a.Title))
                .Select(a => a.Title)
                .ToListAsync(ct);
            foreach (var key in batch)
                titleSet.Add(key);
        }

        return new ExistingArticleKeys(extSet, urlSet, titleSet);
    }
}
