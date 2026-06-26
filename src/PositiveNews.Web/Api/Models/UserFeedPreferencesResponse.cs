namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Saved feed preferences returned to the client.
/// </summary>
public sealed class UserFeedPreferencesResponse
{
    /// <summary>Preferred topic names.</summary>
    public IReadOnlyList<string> TopicNames { get; init; } = Array.Empty<string>();

    /// <summary>Preferred source ids.</summary>
    public IReadOnlyList<int> SourceIds { get; init; } = Array.Empty<int>();

    /// <summary>Minimum positivity score in [0, 1].</summary>
    public decimal MinPositivity { get; init; }

    /// <summary>Sort mode: date, positivity, or preferences.</summary>
    public string SortBy { get; init; } = "date";
}
