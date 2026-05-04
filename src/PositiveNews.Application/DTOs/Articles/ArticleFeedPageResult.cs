namespace PositiveNews.Application.DTOs.Articles;

public sealed class ArticleFeedPageResult
{
    public IReadOnlyList<ArticleFeedItemDto> Articles { get; init; } = Array.Empty<ArticleFeedItemDto>();
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public int PageSize { get; init; }
    public IReadOnlyList<string> SelectedTopics { get; init; } = Array.Empty<string>();
}
