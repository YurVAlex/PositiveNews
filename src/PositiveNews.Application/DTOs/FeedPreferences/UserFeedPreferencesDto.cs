namespace PositiveNews.Application.DTOs.FeedPreferences;

/// <summary>
/// Snapshot of a user's saved feed preferences for API and client sync.
/// </summary>
/// <param name="TopicNames">Preferred topic names (empty when none selected).</param>
/// <param name="SourceIds">Preferred source identifiers (empty when none selected).</param>
/// <param name="MinPositivity">Minimum positivity score in [0, 1].</param>
/// <param name="SortBy">Sort mode: <c>date</c>, <c>positivity</c>, or <c>preferences</c>.</param>
public sealed record UserFeedPreferencesDto(
    IReadOnlyList<string> TopicNames,
    IReadOnlyList<int> SourceIds,
    decimal MinPositivity,
    string SortBy);
