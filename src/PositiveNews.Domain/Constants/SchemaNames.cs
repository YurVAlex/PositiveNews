namespace PositiveNews.Domain.Constants;

/// <summary>
/// Centralizes SQL schema names to avoid magic strings in EF configurations.
/// </summary>
public static class SchemaNames
{
    /// <summary>Identity / authentication-related tables.</summary>
    public const string Identity = "Identity";

    /// <summary>Catalog content (sources, topics, articles).</summary>
    public const string Catalog = "Catalog";

    /// <summary>Community features (comments, preferences).</summary>
    public const string Community = "Community";

    /// <summary>Administrative / audit tables.</summary>
    public const string Admin = "Admin";
}
