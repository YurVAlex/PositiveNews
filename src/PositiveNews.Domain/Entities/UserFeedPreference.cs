namespace PositiveNews.Domain.Entities;

public class UserFeedPreference
{
    public long UserId { get; set; }
    public decimal MinPositivity { get; set; } = 0.5m;
    public string SortBy { get; set; } = "Date";
    public string? LanguageCode { get; set; }
    public string? RegionCode { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}