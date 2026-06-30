using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// Per-user defaults for the article feed: minimum positivity, sort order, and optional locale hints.
/// </summary>
public class UserFeedPreference
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private UserFeedPreference() { }

    /// <summary>User this preference row belongs to (also the primary key).</summary>
    public long UserId { get; private set; }

    /// <summary>Minimum positivity score [0,1] for feed items.</summary>
    public decimal MinPositivity { get; private set; } = FeedPreferenceDefaults.MinPositivity;

    /// <summary>Sort mode identifier (e.g. Date).</summary>
    public string SortBy { get; private set; } = "Date";

    /// <summary>Optional preferred language filter.</summary>
    public string? LanguageCode { get; private set; }

    /// <summary>Optional preferred region filter.</summary>
    public string? RegionCode { get; private set; }

    /// <summary>Owning user.</summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Creates default preferences for the given user id.
    /// </summary>
    public static UserFeedPreference Create(long userId, decimal minPositivity = FeedPreferenceDefaults.MinPositivity, string sortBy = "Date")
    {
        if (minPositivity < 0m || minPositivity > 1m)
            throw new DomainException($"MinPositivity must be between 0 and 1 (got {minPositivity}).");

        return new UserFeedPreference
        {
            UserId = userId,
            MinPositivity = minPositivity,
            SortBy = string.IsNullOrWhiteSpace(sortBy) ? "Date" : sortBy.Trim()
        };
    }

    /// <summary>
    /// Replaces positivity threshold, sort mode, and optional locale filters.
    /// </summary>
    public void UpdatePreferences(decimal minPositivity, string sortBy, string? languageCode, string? regionCode)
    {
        if (minPositivity < 0m || minPositivity > 1m)
            throw new DomainException($"MinPositivity must be between 0 and 1 (got {minPositivity}).");

        MinPositivity = minPositivity;
        SortBy = string.IsNullOrWhiteSpace(sortBy) ? "Date" : sortBy.Trim();
        LanguageCode = languageCode?.Trim();
        RegionCode = regionCode?.Trim();
    }
}
