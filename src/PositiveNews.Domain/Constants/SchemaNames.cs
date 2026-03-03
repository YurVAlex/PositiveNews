namespace PositiveNews.Domain.Constants;

/// <summary>
/// Centralizes SQL schema names to avoid magic strings in EF configurations.
/// </summary>
public static class SchemaNames
{
    public const string Identity = "Identity";
    public const string Catalog = "Catalog";
    public const string Community = "Community";
    public const string Admin = "Admin";
}