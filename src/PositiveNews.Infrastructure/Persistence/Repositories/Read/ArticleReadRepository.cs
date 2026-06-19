using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Ingestion;
using PositiveNews.Application.Mapping;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class ArticleReadRepository(AppDbContext db, ISourceReadRepository sourceReadRepository)
    : IArticleReadRepository
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

        if (filter.MinPositivity.HasValue)
        {
            var threshold = filter.MinPositivity.Value;
            query = query.Where(a => a.PositivityScore == null || a.PositivityScore >= threshold);
        }

        query = filter.SortBy switch
        {
            ArticleFeedSortBy.Preferences => ApplyPreferenceSort(query, topicNamesLower, sourceIds),
            _ => ApplyLegacyPreferenceBoostThenSort(query, topicNamesLower, sourceIds, filter.SortBy)
        };

        var totalArticles = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

        var articles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToArticleFeedItemDto()
            .ToListAsync(ct);

        var selectedSources = await sourceReadRepository.GetSourceFilterItemsByIdsAsync(sourceIds, ct);

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

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleAdminItemDto>> SearchAdminArticlesAsync(string? searchTerm, CancellationToken ct)
    {
        var query = db.ArticlesMetadata
            .Include(a => a.Source)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var trimmed = searchTerm.Trim();
            var searchById = long.TryParse(trimmed, out var articleId);
            query = query.Where(a =>
                (searchById && a.Id == articleId)
                || a.Title.Contains(trimmed)
                || a.Source.Name.Contains(trimmed));
        }

        return await query
            .OrderByDescending(a => a.PublishedAt)
            .Take(50)
            .Select(a => new ArticleAdminItemDto
            {
                Id = a.Id,
                SourceId = a.SourceId,
                SourceName = a.Source.Name,
                Title = a.Title,
                PositivityScore = a.PositivityScore,
                IsActive = a.IsActive,
                ModeratedBy = a.ModeratedBy,
                PublishedAt = a.PublishedAt
            })
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ArticleAdminDetailDto?> GetAdminArticleDetailAsync(long articleId, CancellationToken ct)
    {
        return await db.ArticlesMetadata
            .Include(a => a.Source)
            .AsNoTracking()
            .Where(a => a.Id == articleId)
            .Select(a => new ArticleAdminDetailDto
            {
                Id = a.Id,
                SourceId = a.SourceId,
                SourceName = a.Source.Name,
                SourceLogoUrl = a.Source.LogoUrl,
                Title = a.Title,
                ImageTag = a.ImageTag,
                PositivityScore = a.PositivityScore,
                Author = a.Author,
                PublishedAt = a.PublishedAt,
                Url = a.Url,
                SummaryShort = a.SummaryShort ?? string.Empty,
                ContentRaw = a.Content != null ? a.Content.ContentRaw : null,
                IsActive = a.IsActive,
                ModeratedBy = a.ModeratedBy
            })
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Ranks by preference weight (1 point per matching preferred topic, 1 for preferred source), then date.
    /// </summary>
    private static IQueryable<ArticleMetadata> ApplyPreferenceSort(
        IQueryable<ArticleMetadata> query,
        IReadOnlyList<string> topicNamesLower,
        IReadOnlyList<int> sourceIds)
    {
        if (topicNamesLower.Count == 0 && sourceIds.Count == 0)
        {
            return query.OrderByDescending(a => a.PublishedAt);
        }

        var hasTopics = topicNamesLower.Count > 0;
        var hasSources = sourceIds.Count > 0;

        if (hasTopics && hasSources)
        {
            return query
                .OrderByDescending(a =>
                    a.ArticleTopics.Count(at =>
                        at.Topic != null && topicNamesLower.Contains(at.Topic.Name.ToLower()))
                    + (sourceIds.Contains(a.SourceId) ? 1 : 0))
                .ThenByDescending(a => a.PublishedAt);
        }

        if (hasTopics)
        {
            return query
                .OrderByDescending(a =>
                    a.ArticleTopics.Count(at =>
                        at.Topic != null && topicNamesLower.Contains(at.Topic.Name.ToLower())))
                .ThenByDescending(a => a.PublishedAt);
        }

        return query
            .OrderByDescending(a => sourceIds.Contains(a.SourceId) ? 1 : 0)
            .ThenByDescending(a => a.PublishedAt);
    }

    /// <summary>
    /// Legacy behavior: any preferred topic/source match first, then date or positivity sort.
    /// </summary>
    private static IQueryable<ArticleMetadata> ApplyLegacyPreferenceBoostThenSort(
        IQueryable<ArticleMetadata> query,
        IReadOnlyList<string> topicNamesLower,
        IReadOnlyList<int> sourceIds,
        ArticleFeedSortBy sortBy)
    {
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
            return ApplySecondarySort(ordered, sortBy);
        }

        return ApplyPrimarySort(query, sortBy);
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
    public Task<bool> ExistsActiveAsync(long id, CancellationToken ct)
        => db.ArticlesMetadata
            .AsNoTracking()
            .AnyAsync(a => a.Id == id && a.IsActive, ct);

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
