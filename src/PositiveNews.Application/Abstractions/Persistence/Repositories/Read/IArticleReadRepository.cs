using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.DTOs.Articles;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

/// <summary>
/// Read-only access to articles for feeds, detail views, and ingestion deduplication.
/// </summary>
public interface IArticleReadRepository
{
    /// <summary>
    /// Loads a filtered and paged slice of articles for the article feed UI.
    /// </summary>
    /// <param name="filter">Paging, topics, and sort options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged article summaries and filter metadata.</returns>
    Task<ArticleFeedPageResult> GetFeedPageAsync(ArticleFeedFilter filter, CancellationToken ct);

    /// <summary>
    /// Loads a single article with joined source information for the detail page.
    /// </summary>
    /// <param name="id">Article primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The detail DTO, or <see langword="null"/> when not found.</returns>
    Task<ArticleDetailDto?> GetDetailAsync(long id, CancellationToken ct);

    /// <summary>
    /// Checks whether an active article exists for the given identifier.
    /// </summary>
    /// <param name="id">Article primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when an active article with this id exists.</returns>
    Task<bool> ExistsActiveAsync(long id, CancellationToken ct);

    /// <summary>
    /// Returns articles available for admin moderation, optionally filtered by title or identifier.
    /// </summary>
    /// <param name="searchTerm">Optional title substring search term.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<ArticleAdminItemDto>> SearchAdminArticlesAsync(string? searchTerm, CancellationToken ct);

    /// <summary>
    /// Loads article details used by the admin moderation UI.
    /// </summary>
    /// <param name="articleId">Article identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ArticleAdminDetailDto?> GetAdminArticleDetailAsync(long articleId, CancellationToken ct);

    /// <summary>
    /// Returns keys already stored for the given external IDs, URLs, and titles (batch dedupe lookup).
    /// </summary>
    /// <param name="externalIds">External identifiers from feeds (may contain nulls).</param>
    /// <param name="urls">Canonical article URLs.</param>
    /// <param name="titles">Article titles.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Sets of matching keys present in storage.</returns>
    Task<ExistingArticleKeys> FindExistingKeysAsync(
        IReadOnlyCollection<string?> externalIds,
        IReadOnlyCollection<string> urls,
        IReadOnlyCollection<string> titles,
        CancellationToken ct);
}
