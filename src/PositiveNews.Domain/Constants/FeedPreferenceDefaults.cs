namespace PositiveNews.Domain.Constants;

/// <summary>
/// Default values for user feed preferences when none are stored.
/// </summary>
public static class FeedPreferenceDefaults
{
    /// <summary>Default minimum positivity score in [0, 1] (no filtering).</summary>
    public const decimal MinPositivity = 0m;
}
