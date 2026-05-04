namespace PositiveNews.Application.Abstractions.Persistence.Models;

public enum ArticleFeedSortBy
{
    PublishedAt = 0,
    PositivityScore = 1
}

public sealed record ArticleFeedFilter(
    int Page,
    int PageSize,
    IReadOnlyList<string> Topics,
    ArticleFeedSortBy SortBy = ArticleFeedSortBy.PublishedAt);
