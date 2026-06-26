using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Writes news source entities (administrative or seed scenarios).
/// </summary>
public interface ISourceWriteRepository
{
    /// <summary>
    /// Stages a new source row for insertion on commit.
    /// </summary>
    /// <param name="source">Source aggregate root.</param>
    void Add(Source source);

    /// <summary>
    /// Loads a source for update using tracking semantics.
    /// </summary>
    /// <param name="sourceId">Source identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Source?> GetByIdAsync(int sourceId, CancellationToken ct);
}
