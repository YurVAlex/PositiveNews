namespace PositiveNews.Application.DTOs.Ingestion;

/// <summary>
/// Current ingestion scheduler state for the admin panel.
/// </summary>
public sealed class IngestionCycleStatusDto
{
    /// <summary>True while a full ingestion cycle is executing.</summary>
    public bool IsRunning { get; init; }

    /// <summary>UTC time when the next scheduled cycle is expected, if known.</summary>
    public DateTime? NextRunAtUtc { get; init; }
}
