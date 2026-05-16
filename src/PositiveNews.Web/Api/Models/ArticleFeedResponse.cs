namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Paginated article feed with applied filters and paging metadata.
/// </summary>
public sealed class ArticleFeedResponse
{
    /// <summary>
    /// Gets the articles for the current page.
    /// </summary>
    public IReadOnlyList<ArticlePreviewResponse> Articles { get; init; } = Array.Empty<ArticlePreviewResponse>();

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int CurrentPage { get; init; }

    /// <summary>
    /// Gets the total number of pages for the current filter set.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Gets the maximum number of articles per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the topic filters that were applied to this result.
    /// </summary>
    public IReadOnlyList<string> SelectedTopics { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the preferred sources that were applied to this result.
    /// </summary>
    public IReadOnlyList<FeedSourcePreferenceResponse> SelectedSources { get; init; } = Array.Empty<FeedSourcePreferenceResponse>();
}
