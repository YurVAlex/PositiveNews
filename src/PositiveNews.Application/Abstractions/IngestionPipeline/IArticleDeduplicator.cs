using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Abstractions.Persistence.Models;

namespace PositiveNews.Application.Abstractions.IngestionPipeline;

/// <summary>
/// Detects duplicate feed items against persisted articles and within a single batch.
/// </summary>
public interface IArticleDeduplicator
{
    /// <summary>
    /// Determines whether the item matches any key already stored in the database.
    /// </summary>
    /// <param name="keys">Existing keys loaded for the current batch.</param>
    /// <param name="dto">Parsed RSS item.</param>
    /// <returns><see langword="true"/> when the item should be skipped as a duplicate.</returns>
    bool MatchesExisting(ExistingArticleKeys keys, RssFeedItemDto dto);

    /// <summary>
    /// Determines whether the item collides with another item already accepted in this ingestion pass.
    /// </summary>
    /// <param name="item">Candidate feed item.</param>
    /// <param name="pendingExternalIds">External IDs staged earlier in the loop.</param>
    /// <param name="pendingUrls">URLs staged earlier in the loop.</param>
    /// <param name="pendingTitles">Titles staged earlier in the loop.</param>
    /// <returns><see langword="true"/> when this item duplicates a pending one.</returns>
    bool ConflictsWithPending(
        RssFeedItemDto item,
        HashSet<string> pendingExternalIds,
        HashSet<string> pendingUrls,
        HashSet<string> pendingTitles);
}
