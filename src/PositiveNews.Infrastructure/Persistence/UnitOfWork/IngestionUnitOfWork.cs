using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.UnitOfWork;

internal sealed class IngestionUnitOfWork(AppDbContext db) : IIngestionUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
