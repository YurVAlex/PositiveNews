namespace PositiveNews.Application.Abstractions.Persistence.UnitOfWork;

/// <summary>
/// Unit-of-work boundary for committing changes made through repositories (single DbContext scope).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists pending changes and returns the number of affected rows.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
