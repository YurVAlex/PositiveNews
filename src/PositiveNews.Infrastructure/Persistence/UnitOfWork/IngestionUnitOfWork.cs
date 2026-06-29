using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Scoped unit-of-work for ingestion pipelines writing through <see cref="AppDbContext"/>.
/// </summary>
internal sealed class IngestionUnitOfWork(AppDbContext db) : IIngestionUnitOfWork
{
    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public void ClearPendingChanges() => db.ChangeTracker.Clear();
}
