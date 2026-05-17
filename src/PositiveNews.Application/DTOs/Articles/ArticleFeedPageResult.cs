namespace PositiveNews.Application.DTOs.Articles;

/// <summary>
/// One page of feed results together with paging metadata.
/// </summary>
public sealed class ArticleFeedPageResult
{
    /// <summary>Articles for the current page.</summary>
    public IReadOnlyList<ArticleFeedItemDto> Articles { get; init; } = Array.Empty<ArticleFeedItemDto>();

    /// <summary>Current one-based page index.</summary>
    public int CurrentPage { get; init; }

    /// <summary>Total number of pages for the current filter.</summary>
    public int TotalPages { get; init; }

    /// <summary>Page size used for the query.</summary>
    public int PageSize { get; init; }

    /// <summary>Topic filter applied (may be empty).</summary>
    public IReadOnlyList<string> SelectedTopics { get; init; } = Array.Empty<string>();

    /// <summary>Preferred sources applied (may be empty).</summary>
    public IReadOnlyList<SourceFilterItemDto> SelectedSources { get; init; } = Array.Empty<SourceFilterItemDto>();
}
