using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

/// <summary>
/// Read-only access to news sources configured for ingestion.
/// </summary>
public interface ISourceReadRepository
{
    /// <summary>
    /// Returns snapshot rows for every active source that should be polled for RSS feeds.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ordered or unordered list of ingestion source snapshots.</returns>
    Task<IReadOnlyList<IngestionSourceSnapshot>> GetActiveIngestionSourcesAsync(CancellationToken ct);

    /// <summary>
    /// Returns ids from <paramref name="ids"/> that exist in the sources catalog.
    /// </summary>
    /// <param name="ids">Candidate source ids.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<int>> GetExistingSourceIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct);
}
