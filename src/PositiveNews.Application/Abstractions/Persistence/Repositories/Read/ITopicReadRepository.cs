using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

/// <summary>
/// Read-only access to topic taxonomy data.
/// </summary>
public interface ITopicReadRepository
{
    /// <summary>
    /// Returns topic display names for filters (e.g. sidebar or dropdown UI).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Topic names suitable for display.</returns>
    Task<IReadOnlyList<string>> GetTopicNamesAsync(CancellationToken ct);

    /// <summary>
    /// Loads every topic row needed to build the ingestion <see cref="TopicLookup"/>.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Immutable snapshots for all topics.</returns>
    Task<IReadOnlyList<TopicSnapshot>> GetAllTopicSnapshotsAsync(CancellationToken ct);

    /// <summary>
    /// Resolves topic names from the feed to database IDs for linking articles.
    /// </summary>
    /// <param name="names">Topic names referenced during ingestion.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Map of topic name to topic identifier.</returns>
    Task<IReadOnlyDictionary<string, int>> GetTopicIdsByNamesAsync(IReadOnlyCollection<string> names, CancellationToken ct);
}
