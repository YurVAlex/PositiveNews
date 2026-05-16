namespace PositiveNews.Application.DTOs.Articles;

/// <summary>
/// Source metadata echoed in feed responses for preferred-source UI chips.
/// </summary>
public sealed class FeedSourcePreferenceDto
{
    /// <summary>Source primary key.</summary>
    public int Id { get; init; }

    /// <summary>Display name of the news source.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional logo URL for the source.</summary>
    public string? LogoUrl { get; init; }
}
