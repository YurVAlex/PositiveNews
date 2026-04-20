namespace PositiveNews.Application.DTOs.Articles;

public sealed class ArticleDetailDto
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string SourceName { get; init; } = string.Empty;
    public string? SourceLogoUrl { get; init; }
    public string? Author { get; init; }
    public DateTime PublishedAt { get; init; }
    public string? ContentHtml { get; init; }
}
