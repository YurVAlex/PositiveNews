namespace PositiveNews.Application.Abstractions.Persistence.Models;

/// <summary>
/// Controls how article feed queries are sorted.
/// </summary>
public enum ArticleFeedSortBy
{
    /// <summary>Order by publication date (newest first within paging).</summary>
    PublishedAt = 0,

    /// <summary>Order by positivity score.</summary>
    PositivityScore = 1,

    /// <summary>
    /// Order by preference weight (1 per matching topic, 1 for matching source), then publication date.
    /// </summary>
    Preferences = 2
}

/// <summary>
/// Filtering and paging parameters for loading a page of articles for the public feed.
/// </summary>
/// <param name="Page">One-based page index.</param>
/// <param name="PageSize">Maximum items per page.</param>
/// <param name="Topics">Topic names to prioritize (empty means no topic preference).</param>
/// <param name="SourceIds">Source ids to prioritize (empty means no source preference).</param>
/// <param name="SortBy">Determines the primary sort order.</param>
public sealed record ArticleFeedFilter(
    int Page,
    int PageSize,
    IReadOnlyList<string> Topics,
    IReadOnlyList<int> SourceIds,
    ArticleFeedSortBy SortBy = ArticleFeedSortBy.PublishedAt);
