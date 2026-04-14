namespace PositiveNews.Application.DTOs;

/// <summary>
/// Represents a single item parsed from an RSS feed.
/// This DTO is source-agnostic and carries only the data the ingestion pipeline needs.
/// </summary>
public class RssFeedItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Link { get; init; } = string.Empty;

    public string ContentRaw { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Author { get; init; }
    public string? ContentClean { get; set; }

    public DateTime PublishedDate { get; init; }

    public string? ExternalId { get; init; }
    public string? ImageTag { get; set; }

    public List<string> Topics { get; set; } = [];
    public decimal? PositivityScore { get; set; }
}