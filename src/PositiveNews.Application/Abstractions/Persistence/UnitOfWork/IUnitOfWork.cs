namespace PositiveNews.Application.Abstractions.Persistence.UnitOfWork;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
