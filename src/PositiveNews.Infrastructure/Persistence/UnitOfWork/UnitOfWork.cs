using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.UnitOfWork;

internal sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
