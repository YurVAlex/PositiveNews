namespace PositiveNews.Application.Ingestion;

public static class IngestionPipelineConstants
{
    public static readonly TimeSpan DelayBetweenSources = TimeSpan.FromSeconds(2);

    /// <summary>Articles inserted per SaveChanges batch (metadata + content, then topics).</summary>
    public const int ArticlePersistChunkSize = 25;

    /// <summary>SQL Server parameter budget — split IN clauses when lists are large.</summary>
    public const int SqlInClauseChunkSize = 500;
}
