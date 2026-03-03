namespace PositiveNews.Application.DTOs;

/// <summary>
/// Represents a single item parsed from an RSS feed.
/// This DTO is source-agnostic and carries only the data the ingestion pipeline needs.
/// </summary>
public class RssFeedItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? ExternalId { get; set; }     // RSS <guid> or <link> as fallback
    public string? ImageUrl { get; set; }
}