using PositiveNews.Application.Abstractions.IngestionPipeline;

namespace PositiveNews.Application.Tests.TestSupport;

/// <summary>
/// Coordinator stub that always permits a new cycle (for handler unit tests).
/// </summary>
internal sealed class AllowingIngestionCycleCoordinator : IIngestionCycleCoordinator
{
    public bool IsRunning { get; private set; }

    public DateTime? NextRunAtUtc { get; private set; }

    public bool TryBeginCycle()
    {
        if (IsRunning) return false;
        IsRunning = true;
        return true;
    }

    public void EndCycle() => IsRunning = false;

    public void SetNextRunAtUtc(DateTime utc) => NextRunAtUtc = utc;
}
