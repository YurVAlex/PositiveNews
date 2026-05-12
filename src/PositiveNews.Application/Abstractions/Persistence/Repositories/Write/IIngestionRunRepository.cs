using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Persists ingestion run audit rows for each RSS poll.
/// </summary>
public interface IIngestionRunRepository
{
    /// <summary>
    /// Stages a new ingestion run for insertion on commit.
    /// </summary>
    /// <param name="run">Run entity capturing status and counts.</param>
    void Add(IngestionRun run);
}
