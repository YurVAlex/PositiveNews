namespace PositiveNews.Application.DTOs.Articles;

/// <summary>
/// Full article payload for the detail page, including body HTML.
/// </summary>
public sealed class ArticleDetailDto
{
    /// <summary>Article primary key.</summary>
    public long Id { get; init; }

    /// <summary>Article headline.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Display name of the originating news source.</summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>Optional logo URL for the source.</summary>
    public string? SourceLogoUrl { get; init; }

    /// <summary>Optional author byline.</summary>
    public string? Author { get; init; }

    /// <summary>Publication timestamp in UTC.</summary>
    public DateTime PublishedAt { get; init; }

    /// <summary>Sanitized HTML body for reading view.</summary>
    public string? ContentHtml { get; init; }
}
