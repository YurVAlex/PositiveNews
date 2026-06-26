namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Represents source details for editing in the admin UI.
/// </summary>
public sealed class SourceAdminDetailResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TrustScore { get; init; }
    public bool IsActive { get; init; }
    public string FeedUrl { get; init; } = string.Empty;
    public long? ModeratedBy { get; init; }
}
