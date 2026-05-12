namespace PositiveNews.Application.DTOs.Articles;

/// <summary>
/// Summary row for one article in the public feed list.
/// </summary>
public sealed class ArticleFeedItemDto
{
    /// <summary>Article primary key.</summary>
    public long Id { get; init; }

    /// <summary>Display name of the news source.</summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>Optional logo URL for the source.</summary>
    public string? SourceLogoUrl { get; init; }

    /// <summary>Article headline.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Optional author byline.</summary>
    public string? Author { get; init; }

    /// <summary>Publication timestamp (typically UTC).</summary>
    public DateTime PublishedAt { get; init; }

    /// <summary>Optional hero image markup or URL snippet.</summary>
    public string? ImageTag { get; init; }

    /// <summary>Short plain-text or HTML summary for cards.</summary>
    public string SummaryShort { get; init; } = string.Empty;

    /// <summary>Canonical article URL on the original site.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Optional positivity score for sorting or badges.</summary>
    public decimal? PositivityScore { get; init; }

    /// <summary>Topic labels displayed as chips or filters.</summary>
    public IReadOnlyList<string> Topics { get; init; } = Array.Empty<string>();
}
