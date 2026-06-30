namespace PositiveNews.Domain.Constants;

/// <summary>
/// Default language and region codes used when values are not specified.
/// </summary>
public static class LanguageDefaults
{
    /// <summary>Default language for new ingestion sources.</summary>
    public const string SourceDefault = "en";

    /// <summary>Placeholder for undetermined article language (BCP 47 und).</summary>
    public const string Undetermined = "und";

    /// <summary>Default region scope for articles.</summary>
    public const string GlobalRegion = "Global";
}
