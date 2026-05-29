namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Represents detailed article metadata returned for admin moderation.
/// </summary>
public sealed class ArticleAdminDetailResponse
{
    public long Id { get; init; }
    public int SourceId { get; init; }
    public string SourceName { get; init; } = string.Empty;
    public string? SourceLogoUrl { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Author { get; init; }
    public DateTime PublishedAt { get; init; }
    public string Url { get; init; } = string.Empty;
    public string SummaryShort { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public long? ModeratedBy { get; init; }
}
