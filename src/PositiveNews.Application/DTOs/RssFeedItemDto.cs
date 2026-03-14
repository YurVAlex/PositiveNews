namespace PositiveNews.Application.DTOs;

/// <summary>
/// Represents a single item parsed from an RSS feed.
/// This DTO is source-agnostic and carries only the data the ingestion pipeline needs.
/// </summary>
public class RssFeedItemDto
{
    public required string Title { get; init; }
    public required string Link { get; init; }

    public required string ContentRaw { get; init; }
    public required string Description { get; init; }
    public string? Author { get; init; }

    public DateTime? PublishedDate { get; init; }

    public string? ExternalId { get; init; }
    public string? ImageUrl { get; init; }

    public List<string>? Topics { get; init; } 
}