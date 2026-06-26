namespace PositiveNews.Application.DTOs.Admin;

/// <summary>
/// Admin-facing article detail returned for moderation.
/// </summary>
public sealed class ArticleAdminDetailDto
{
    public long Id { get; init; }
    public int SourceId { get; init; }
    public string SourceName { get; init; } = string.Empty;
    public string? SourceLogoUrl { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? ImageTag { get; init; }
    public decimal? PositivityScore { get; init; }
    public string? Author { get; init; }
    public DateTime PublishedAt { get; init; }
    public string Url { get; init; } = string.Empty;
    public string SummaryShort { get; init; } = string.Empty;
    public string? ContentRaw { get; init; }
    public bool IsActive { get; init; }
    public long? ModeratedBy { get; init; }
}
