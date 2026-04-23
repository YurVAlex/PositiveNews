using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

public class UserFeedPreference
{
    // For EF Core materialization
    private UserFeedPreference() { }

    public long UserId { get; private set; }
    public decimal MinPositivity { get; private set; } = 0.5m;
    public string SortBy { get; private set; } = "Date";
    public string? LanguageCode { get; private set; }
    public string? RegionCode { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    public static UserFeedPreference Create(long userId, decimal minPositivity = 0.5m, string sortBy = "Date")
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
