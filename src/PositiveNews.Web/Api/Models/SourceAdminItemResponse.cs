namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Represents a source row in the admin management table.
/// </summary>
public sealed class SourceAdminItemResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TrustScore { get; init; }
    public bool IsActive { get; init; }
    public long? ModeratedBy { get; init; }
}
