namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Full article payload including HTML body for detail views.
/// </summary>
public sealed class ArticleDetailResponse
{
    /// <summary>
    /// Gets the unique article identifier.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the article headline.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of the content source.
    /// </summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>
    /// Gets an optional URL for the source logo image.
    /// </summary>
    public string? SourceLogoUrl { get; init; }

    /// <summary>
    /// Gets the author name when available.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Gets the publication timestamp in UTC.
    /// </summary>
    public DateTime PublishedAt { get; init; }

    /// <summary>
    /// Gets sanitized HTML body content when available.
    /// </summary>
    public string? ContentHtml { get; init; }
}
