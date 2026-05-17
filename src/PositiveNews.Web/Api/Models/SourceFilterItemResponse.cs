namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Source metadata for filter UI and preferred-source chips.
/// </summary>
public sealed class SourceFilterItemResponse
{
    /// <summary>
    /// Gets the source identifier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the display name of the news source.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets an optional logo URL for the source.
    /// </summary>
    public string? LogoUrl { get; init; }
}
