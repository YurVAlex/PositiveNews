namespace PositiveNews.Application.Abstractions.Persistence.Models;

/// <summary>
/// Controls how article feed queries are sorted.
/// </summary>
public enum ArticleFeedSortBy
{
    /// <summary>Order by publication date (newest first within paging).</summary>
    PublishedAt = 0,

    /// <summary>Order by positivity score.</summary>
    PositivityScore = 1
}

/// <summary>
/// Filtering and paging parameters for loading a page of articles for the public feed.
/// </summary>
/// <param name="Page">One-based page index.</param>
/// <param name="PageSize">Maximum items per page.</param>
/// <param name="Topics">Topic names to filter by (empty means no topic filter).</param>
/// <param name="SortBy">Determines the primary sort order.</param>
public sealed record ArticleFeedFilter(
    int Page,
    int PageSize,
    IReadOnlyList<string> Topics,
    ArticleFeedSortBy SortBy = ArticleFeedSortBy.PublishedAt);
