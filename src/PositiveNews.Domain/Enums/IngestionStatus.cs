namespace PositiveNews.Domain.Enums;

/// <summary>
/// Lifecycle state for a feed <see cref="Entities.IngestionRun"/>.
/// </summary>
public enum IngestionStatus
{
    /// <summary>In progress.</summary>
    Running,

    /// <summary>Finished successfully.</summary>
    Success,

    /// <summary>Finished with an error.</summary>
    Failed,

    /// <summary>Ended without full success (e.g. partial fetch).</summary>
    Partial
}
