using PositiveNews.Domain.Enums;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

public class IngestionRun
{
    // For EF Core materialization
    private IngestionRun() { }

    public long Id { get; private set; }
    public int SourceId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public IngestionStatus Status { get; private set; }
    public int ItemsFetched { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Navigation
    public Source Source { get; private set; } = null!;

    /// <summary>Creates a new ingestion run in the Running state.</summary>
    public static IngestionRun Start(int sourceId)
    {
        return new IngestionRun
        {
            SourceId = sourceId,
            StartedAt = DateTime.UtcNow,
            Status = IngestionStatus.Running
        };
    }

    /// <summary>Transitions to Success.</summary>
    public void Complete(int itemsFetched)
    {
        EnsureRunning(IngestionStatus.Success);
        Status = IngestionStatus.Success;
        ItemsFetched = itemsFetched;
        FinishedAt = DateTime.UtcNow;
    }

    /// <summary>Transitions to Partial (e.g. cancelled or empty feed).</summary>
    public void PartialComplete(int itemsFetched, string? reason = null)
    {
        EnsureRunning(IngestionStatus.Partial);
        Status = IngestionStatus.Partial;
        ItemsFetched = itemsFetched;
        ErrorMessage = reason;
        FinishedAt = DateTime.UtcNow;
    }

    /// <summary>Transitions to Failed.</summary>
    public void Fail(string errorMessage, int itemsFetched = 0)
    {
        EnsureRunning(IngestionStatus.Failed);
        Status = IngestionStatus.Failed;
        ItemsFetched = itemsFetched;
        ErrorMessage = errorMessage.Length > 4000 ? errorMessage[..4000] : errorMessage;
        FinishedAt = DateTime.UtcNow;
    }

    private void EnsureRunning(IngestionStatus target)
    {
        if (Status != IngestionStatus.Running)
            throw new InvalidIngestionTransitionException(Status, target);
    }
}
