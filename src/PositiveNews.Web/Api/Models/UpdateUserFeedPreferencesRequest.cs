namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Full feed preference snapshot submitted by the client.
/// </summary>
public sealed class UpdateUserFeedPreferencesRequest
{
    /// <summary>Preferred topic names.</summary>
    public IReadOnlyList<string> TopicNames { get; init; } = Array.Empty<string>();

    /// <summary>Preferred source ids.</summary>
    public IReadOnlyList<int> SourceIds { get; init; } = Array.Empty<int>();

    /// <summary>Minimum positivity score in [0, 1].</summary>
    public decimal MinPositivity { get; init; } = 0.5m;

    /// <summary>Sort mode: date, positivity, or preferences.</summary>
    public string SortBy { get; init; } = "date";
}
