namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Lightweight article summary returned in feed listings.
/// </summary>
public sealed class ArticlePreviewResponse
{
    /// <summary>
    /// Gets the unique article identifier.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the display name of the content source.
    /// </summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>
    /// Gets an optional URL for the source logo image.
    /// </summary>
    public string? SourceLogoUrl { get; init; }

    /// <summary>
    /// Gets the article headline.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the author name when available.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Gets the publication timestamp in UTC.
    /// </summary>
    public DateTime PublishedAt { get; init; }

    /// <summary>
    /// Gets a short tag describing the hero image, when present.
    /// </summary>
    public string? ImageTag { get; init; }

    /// <summary>
    /// Gets a brief plain-text summary for cards and lists.
    /// </summary>
    public string SummaryShort { get; init; } = string.Empty;

    /// <summary>
    /// Gets the canonical URL to the full article.
    /// </summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// Gets an optional positivity score for ranked feeds.
    /// </summary>
    public decimal? PositivityScore { get; init; }

    /// <summary>
    /// Gets topic labels associated with the article.
    /// </summary>
    public IReadOnlyList<string> Topics { get; init; } = Array.Empty<string>();
}
