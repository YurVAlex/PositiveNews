using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Abstractions.IngestionPipeline;

/// <summary>
/// Supplies the current ingestion configuration as an immutable snapshot.
/// </summary>
public interface IIngestionSettingsProvider
{
    /// <summary>
    /// Builds a snapshot of positivity, cleaner, validation, and per-source rules.
    /// </summary>
    /// <returns>Immutable settings used by the ingestion pipeline.</returns>
    IngestionSettingsSnapshot GetCurrentSettings();
}
