namespace PositiveNews.Application.DTOs;

/// <summary>
/// Represents a single item parsed from an RSS feed.
/// This DTO is source-agnostic and carries only the data the ingestion pipeline needs.
/// </summary>
public sealed record RssFeedItemDto
{
    public string Title { get; init; } = string.Empty;
    public string Link { get; init; } = string.Empty;

    public string ContentRaw { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Author { get; init; }
    public string? ContentClean { get; init; }

    public DateTime PublishedDate { get; init; }

    public string? ExternalId { get; init; }
    public string? ImageTag { get; init; }

    public IReadOnlyList<string> Topics { get; init; } = Array.Empty<string>();
    public decimal? PositivityScore { get; init; }
}
