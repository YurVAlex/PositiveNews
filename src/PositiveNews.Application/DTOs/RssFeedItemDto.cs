namespace PositiveNews.Application.DTOs;

/// <summary>
/// Represents a single item parsed from an RSS feed.
/// This DTO is source-agnostic and carries only the data the ingestion pipeline needs.
/// </summary>
public sealed record RssFeedItemDto
{
    /// <summary>Article headline from the feed.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Canonical link to the article on the publisher site.</summary>
    public string Link { get; init; } = string.Empty;

    /// <summary>Raw HTML or text body from <c>content:encoded</c> or equivalent.</summary>
    public string ContentRaw { get; init; } = string.Empty;

    /// <summary>RSS description or summary field.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Optional author or dc:creator value.</summary>
    public string? Author { get; init; }

    /// <summary>Plain-text or stripped HTML used for sentiment after cleaning.</summary>
    public string? ContentClean { get; init; }

    /// <summary>Publication date from the feed item.</summary>
    public DateTime PublishedDate { get; init; }

    /// <summary>Stable id from GUID or permalink when present.</summary>
    public string? ExternalId { get; init; }

    /// <summary>Hero image tag or embedding snippet selected during processing.</summary>
    public string? ImageTag { get; init; }

    /// <summary>Normalized topic names after enrichment.</summary>
    public IReadOnlyList<string> Topics { get; init; } = Array.Empty<string>();

    /// <summary>Computed positivity score for persistence.</summary>
    public decimal? PositivityScore { get; init; }
}
