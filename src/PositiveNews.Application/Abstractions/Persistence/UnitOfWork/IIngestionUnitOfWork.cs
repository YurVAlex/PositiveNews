namespace PositiveNews.Application.Abstractions.Persistence.UnitOfWork;

/// <summary>
/// Unit-of-work used during ingestion so feed processing can commit independently of the main app UoW when needed.
/// </summary>
public interface IIngestionUnitOfWork
{
    /// <summary>
    /// Persists ingestion-related changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears tracked entities after a failed save so callers can retry with a clean change tracker.
    /// </summary>
    void ClearPendingChanges();
}
