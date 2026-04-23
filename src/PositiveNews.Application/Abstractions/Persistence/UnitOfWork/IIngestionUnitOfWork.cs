namespace PositiveNews.Application.Abstractions.Persistence.UnitOfWork;

public interface IIngestionUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
