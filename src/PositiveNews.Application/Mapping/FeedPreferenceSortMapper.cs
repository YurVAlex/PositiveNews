namespace PositiveNews.Application.Mapping;

/// <summary>
/// Maps feed sort values between API wire format and persisted preference strings.
/// </summary>
public static class FeedPreferenceSortMapper
{
    /// <summary>Default sort when nothing is stored.</summary>
    public const string DefaultApiSort = "date";

    /// <summary>
    /// Converts a persisted sort value to the API/client sort token.
    /// </summary>
    public static string ToApiSort(string? storedSort)
    {
        if (string.IsNullOrWhiteSpace(storedSort))
        {
            return DefaultApiSort;
        }

        if (string.Equals(storedSort, "Positivity", StringComparison.OrdinalIgnoreCase)
            || string.Equals(storedSort, "PositivityScore", StringComparison.OrdinalIgnoreCase))
        {
            return "positivity";
        }

        if (string.Equals(storedSort, "Preferences", StringComparison.OrdinalIgnoreCase))
        {
            return "preferences";
        }

        return DefaultApiSort;
    }

    /// <summary>
    /// Converts an API/client sort token to the persisted preference string.
    /// </summary>
    public static string ToStoredSort(string? apiSort)
    {
        if (string.Equals(apiSort, "positivity", StringComparison.OrdinalIgnoreCase))
        {
            return "Positivity";
        }

        if (string.Equals(apiSort, "preferences", StringComparison.OrdinalIgnoreCase))
        {
            return "Preferences";
        }

        return "Date";
    }

    /// <summary>
    /// Returns true when the API sort token is supported.
    /// </summary>
    public static bool IsValidApiSort(string? apiSort)
    {
        if (string.IsNullOrWhiteSpace(apiSort))
        {
            return true;
        }

        return string.Equals(apiSort, "date", StringComparison.OrdinalIgnoreCase)
            || string.Equals(apiSort, "positivity", StringComparison.OrdinalIgnoreCase)
            || string.Equals(apiSort, "preferences", StringComparison.OrdinalIgnoreCase);
    }
}
