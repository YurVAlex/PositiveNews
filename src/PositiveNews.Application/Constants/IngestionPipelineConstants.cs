namespace PositiveNews.Application.Constants;

/// <summary>
/// Shared timing and batching constants for RSS ingestion orchestration.
/// </summary>
public static class IngestionPipelineConstants
{
    /// <summary>Pause inserted between processing consecutive sources to reduce upstream throttling.</summary>
    public static readonly TimeSpan DelayBetweenSources = TimeSpan.FromSeconds(2);

    /// <summary>Articles inserted per SaveChanges batch (metadata + content, then topics).</summary>
    public const int ArticlePersistChunkSize = 25;

    /// <summary>SQL Server parameter budget — split IN clauses when lists are large.</summary>
    public const int SqlInClauseChunkSize = 500;
}
