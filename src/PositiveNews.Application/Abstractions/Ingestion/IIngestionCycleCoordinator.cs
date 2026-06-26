namespace PositiveNews.Application.Abstractions.Ingestion;

/// <summary>
/// Tracks ingestion cycle execution and scheduled next run for admin UI and concurrency control.
/// </summary>
public interface IIngestionCycleCoordinator
{
    /// <summary>True while a full ingestion cycle is executing.</summary>
    bool IsRunning { get; }

    /// <summary>UTC time when the background service expects the next scheduled cycle, if known.</summary>
    DateTime? NextRunAtUtc { get; }

    /// <summary>Attempts to mark a cycle as started. Returns false if a cycle is already running.</summary>
    bool TryBeginCycle();

    /// <summary>Marks the current cycle as finished.</summary>
    void EndCycle();

    /// <summary>Updates the next scheduled cycle time (typically after a cycle completes).</summary>
    void SetNextRunAtUtc(DateTime utc);
}
