using PositiveNews.Application.Constants;

namespace PositiveNews.Application.Options;

/// <summary>
/// Article feed paging options bound from configuration.
/// </summary>
public sealed class ArticleFeedOptions
{
    /// <summary>Configuration section name (<c>ArticleFeed</c>).</summary>
    public const string SectionName = "ArticleFeed";

    /// <summary>Default items per page when the client omits <c>pageSize</c>.</summary>
    public int DefaultPageSize { get; init; } = PaginationConstants.DefaultPageSize;
}
