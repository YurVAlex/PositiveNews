namespace PositiveNews.Application.DTOs.Admin;

/// <summary>
/// Admin-facing source details used for editing.
/// </summary>
public sealed class SourceAdminDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TrustScore { get; init; }
    public bool IsActive { get; init; }
    public string FeedUrl { get; init; } = string.Empty;
    public long? ModeratedBy { get; init; }
}
