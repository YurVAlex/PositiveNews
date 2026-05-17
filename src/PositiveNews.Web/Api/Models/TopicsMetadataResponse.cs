namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Topic names exposed for building article filter UI.
/// </summary>
public sealed class TopicsMetadataResponse
{
    /// <summary>
    /// Gets all topic names available for filtering.
    /// </summary>
    public IReadOnlyList<string> TopicNames { get; init; } = Array.Empty<string>();
}
