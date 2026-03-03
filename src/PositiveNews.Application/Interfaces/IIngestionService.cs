namespace PositiveNews.Application.Interfaces;

/// <summary>
/// Orchestrates the ingestion cycle: fetches feeds from all active sources,
/// deduplicates, and persists new articles.
/// </summary>
public interface IIngestionService
{
    Task RunIngestionCycleAsync(CancellationToken cancellationToken = default);
}