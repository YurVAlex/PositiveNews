using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

/// <summary>
/// Read-only access to ingestion run history.
/// </summary>
public interface IIngestionRunReadRepository
{
    /// <summary>
    /// Returns the most recent ingestion runs, newest first.
    /// </summary>
    /// <param name="limit">Maximum number of rows to return.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<IngestionRunListItemDto>> GetLatestAsync(int limit, CancellationToken ct);
}
