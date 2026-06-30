namespace PositiveNews.Application.Ingestion;

/// <summary>
/// Catalog topic names used during RSS ingestion.
/// </summary>
public static class IngestionCatalogConstants
{
    /// <summary>Fallback topic when feeds omit categories or enrichment finds no matches.</summary>
    public const string DefaultTopicName = "Default";
}
