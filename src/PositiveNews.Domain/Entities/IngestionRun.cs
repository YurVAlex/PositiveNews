using PositiveNews.Domain.Enums;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// Tracks a single RSS/feed ingestion attempt for a <see cref="Source"/>: timing, outcome, and counters.
/// </summary>
public class IngestionRun
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private IngestionRun() { }

    /// <summary>Primary key.</summary>
    public long Id { get; private set; }

    /// <summary>Source ingested in this run.</summary>
    public int SourceId { get; private set; }

    /// <summary>When processing started.</summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>When processing finished (success, partial, or failure).</summary>
    public DateTime? FinishedAt { get; private set; }

    /// <summary>Lifecycle state of the run.</summary>
    public IngestionStatus Status { get; private set; }

    /// <summary>Number of feed items successfully processed in this run.</summary>
    public int ItemsFetched { get; private set; }

    /// <summary>Error or diagnostic message when status is not success.</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Navigation to the source.</summary>
    public Source Source { get; private set; } = null!;

    /// <summary>
    /// Starts a new ingestion run in the <see cref="IngestionStatus.Running"/> state.
    /// </summary>
    public static IngestionRun Start(int sourceId)
    {
        return new IngestionRun
        {
            SourceId = sourceId,
            StartedAt = DateTime.UtcNow,
            Status = IngestionStatus.Running
        };
    }

    /// <summary>
    /// Marks the run as successfully completed with the given item count.
    /// </summary>
    public void Complete(int itemsFetched)
    {
        EnsureRunning(IngestionStatus.Success);
        Status = IngestionStatus.Success;
        ItemsFetched = itemsFetched;
        FinishedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the run as partially completed (e.g. cancelled mid-run or empty feed) with an optional reason.
    /// </summary>
    public void PartialComplete(int itemsFetched, string? reason = null)
    {
        EnsureRunning(IngestionStatus.Partial);
        Status = IngestionStatus.Partial;
        ItemsFetched = itemsFetched;
        ErrorMessage = reason;
        FinishedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the run as failed with an error message and optional partial item count.
    /// </summary>
    public void Fail(string errorMessage, int itemsFetched = 0)
    {
        EnsureRunning(IngestionStatus.Failed);
        Status = IngestionStatus.Failed;
        ItemsFetched = itemsFetched;
        ErrorMessage = errorMessage.Length > 4000 ? errorMessage[..4000] : errorMessage;
        FinishedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ensures the run is still <see cref="IngestionStatus.Running"/> before transitioning to <paramref name="target"/>.
    /// </summary>
    private void EnsureRunning(IngestionStatus target)
    {
        if (Status != IngestionStatus.Running)
            throw new InvalidIngestionTransitionException(Status, target);
    }
}
