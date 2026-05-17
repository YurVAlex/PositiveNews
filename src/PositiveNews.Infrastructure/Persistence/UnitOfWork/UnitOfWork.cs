using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Default unit-of-work implementation delegating persistence to <see cref="AppDbContext"/>.
/// </summary>
internal sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
