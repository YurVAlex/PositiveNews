using PositiveNews.Application.Abstractions.IngestionPipeline;

namespace PositiveNews.Infrastructure.Ingestion;

/// <inheritdoc />
internal sealed class IngestionCycleCoordinator : IIngestionCycleCoordinator
{
    private readonly Lock _lock = new();
    private int _running;
    private DateTime? _nextRunAtUtc;

    /// <inheritdoc />
    public bool IsRunning => Volatile.Read(ref _running) == 1;

    /// <inheritdoc />
    public DateTime? NextRunAtUtc
    {
        get
        {
            lock (_lock)
            {
                return _nextRunAtUtc;
            }
        }
    }

    /// <inheritdoc />
    public bool TryBeginCycle()
    {
        return Interlocked.CompareExchange(ref _running, 1, 0) == 0;
    }

    /// <inheritdoc />
    public void EndCycle()
    {
        Interlocked.Exchange(ref _running, 0);
    }

    /// <inheritdoc />
    public void SetNextRunAtUtc(DateTime utc)
    {
        lock (_lock)
        {
            _nextRunAtUtc = utc;
        }
    }
}
