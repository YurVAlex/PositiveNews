namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Represents an article row in the admin moderation table.
/// </summary>
public sealed class ArticleAdminItemResponse
{
    public long Id { get; init; }
    public int SourceId { get; init; }
    public string SourceName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public long? ModeratedBy { get; init; }
    public DateTime PublishedAt { get; init; }
}
