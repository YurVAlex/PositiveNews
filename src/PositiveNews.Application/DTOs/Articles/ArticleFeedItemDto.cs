namespace PositiveNews.Application.DTOs.Articles;

public sealed class ArticleFeedItemDto
{
    public long Id { get; init; }
    public string SourceName { get; init; } = string.Empty;
    public string? SourceLogoUrl { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Author { get; init; }
    public DateTime PublishedAt { get; init; }
    public string? ImageTag { get; init; }
    public string SummaryShort { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public decimal? PositivityScore { get; init; }
    public IReadOnlyList<string> Topics { get; init; } = Array.Empty<string>();
}
