namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Sources exposed for building article filter UI.
/// </summary>
public sealed class SourcesMetadataResponse
{
    /// <summary>
    /// Gets all active sources available for filtering.
    /// </summary>
    public IReadOnlyList<SourceFilterItemResponse> Sources { get; init; } = Array.Empty<SourceFilterItemResponse>();
}
