namespace PositiveNews.Application.DTOs.Articles;

/// <summary>
/// Source row exposed for feed filter UI and preferred-source chips.
/// </summary>
public sealed class SourceFilterItemDto
{
    /// <summary>Source primary key.</summary>
    public int Id { get; init; }

    /// <summary>Display name of the news source.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional logo URL for the source.</summary>
    public string? LogoUrl { get; init; }
}
