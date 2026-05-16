using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Ingestion;
using PositiveNews.Application.Mapping;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class ArticleReadRepository(AppDbContext db) : IArticleReadRepository
{
    /// <inheritdoc />
    public async Task<ArticleFeedPageResult> GetFeedPageAsync(ArticleFeedFilter filter, CancellationToken ct)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var topics = (filter.Topics ?? Array.Empty<string>())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var topicNamesLower = topics.Select(t => t.ToLowerInvariant()).ToList();
        var sourceIds = (filter.SourceIds ?? Array.Empty<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var query = db.ArticlesMetadata
            .Where(a => a.IsActive)
            .AsNoTracking();

        IOrderedQueryable<ArticleMetadata>? ordered = null;

        if (topicNamesLower.Count > 0)
        {
            ordered = query.OrderByDescending(a => a.ArticleTopics.Any(at =>
                at.Topic != null && topicNamesLower.Contains(at.Topic.Name.ToLower())));
        }

        if (sourceIds.Count > 0)
        {
            ordered = ordered == null
                ? query.OrderByDescending(a => sourceIds.Contains(a.SourceId))
                : ordered.ThenByDescending(a => sourceIds.Contains(a.SourceId));
        }

        if (ordered != null)
        {
            query = ApplySecondarySort(ordered, filter.SortBy);
        }
        else
        {
            query = ApplyPrimarySort(query, filter.SortBy);
        }

        var totalArticles = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

        var articles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToArticleFeedItemDto()
            .ToListAsync(ct);

        var selectedSources = await LoadSelectedSourcesAsync(sourceIds, ct);

        return new ArticleFeedPageResult
        {
            Articles = articles,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            SelectedTopics = topics,
            SelectedSources = selectedSources
        };
    }

    private async Task<IReadOnlyList<FeedSourcePreferenceDto>> LoadSelectedSourcesAsync(
        IReadOnlyList<int> sourceIds,
        CancellationToken ct)
    {
        if (sourceIds.Count == 0)
        {
            return Array.Empty<FeedSourcePreferenceDto>();
        }

        var rows = await db.Sources
            .AsNoTracking()
            .Where(s => sourceIds.Contains(s.Id))
            .Select(s => new FeedSourcePreferenceDto
            {
                Id = s.Id,
                Name = s.Name,
                LogoUrl = s.LogoUrl
            })
            .ToListAsync(ct);

        var byId = rows.ToDictionary(s => s.Id);
        return sourceIds
            .Where(byId.ContainsKey).Select(id => byId[id]).ToList();
    }

    private static IQueryable<ArticleMetadata> ApplyPrimarySort(
        IQueryable<ArticleMetadata> query,
        ArticleFeedSortBy sortBy)
    {
        return sortBy == ArticleFeedSortBy.PositivityScore
            ? query
                .OrderByDescending(a => a.PositivityScore != null)
                .ThenByDescending(a => a.PositivityScore ?? 0m)
            : query.OrderByDescending(a => a.PublishedAt);
    }

    private static IOrderedQueryable<ArticleMetadata> ApplySecondarySort(
        IOrderedQueryable<ArticleMetadata> query,
        ArticleFeedSortBy sortBy)
    {
        return sortBy == ArticleFeedSortBy.PositivityScore
            ? query
                .ThenByDescending(a => a.PositivityScore != null)
                .ThenByDescending(a => a.PositivityScore ?? 0m)
            : query.ThenByDescending(a => a.PublishedAt);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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
