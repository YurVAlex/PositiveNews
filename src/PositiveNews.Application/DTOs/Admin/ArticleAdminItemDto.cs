namespace PositiveNews.Application.DTOs.Admin;

/// <summary>
/// Admin-facing article row for moderation listings.
/// </summary>
public sealed class ArticleAdminItemDto
{
    public long Id { get; init; }
    public int SourceId { get; init; }
    public string SourceName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public decimal? PositivityScore { get; init; }
    public bool IsActive { get; init; }
    public long? ModeratedBy { get; init; }
    public DateTime PublishedAt { get; init; }
}
