namespace PositiveNews.Web.Api.Models;

public sealed class ArticleFeedResponse
{
    public IReadOnlyList<ArticlePreviewResponse> Articles { get; init; } = Array.Empty<ArticlePreviewResponse>();
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public int PageSize { get; init; }
    public IReadOnlyList<string> SelectedTopics { get; init; } = Array.Empty<string>();
}
