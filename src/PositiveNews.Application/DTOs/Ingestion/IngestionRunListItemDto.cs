namespace PositiveNews.Application.DTOs.Ingestion;

/// <summary>
/// Row for the admin ingestion runs table.
/// </summary>
public sealed class IngestionRunListItemDto
{
    /// <summary>Run primary key.</summary>
    public long Id { get; init; }

    /// <summary>Display name of the ingested source.</summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>When processing started (UTC).</summary>
    public DateTime StartedAt { get; init; }

    /// <summary>When processing finished (UTC), if completed.</summary>
    public DateTime? FinishedAt { get; init; }

    /// <summary>Lifecycle status name.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Number of items fetched in this run.</summary>
    public int ItemsFetched { get; init; }
}
