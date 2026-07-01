using PositiveNews.Application.DTOs.Ingestion;
using System.Xml.Linq;

namespace PositiveNews.Application.Abstractions.IngestionPipeline;

/// <summary>
/// End-to-end transformation of a fetched RSS document into validated, enriched DTOs.
/// </summary>
public interface IFeedProcessor
{
    /// <summary>
    /// Iterates feed items, parses them, validates, cleans, scores sentiment, and collects results.
    /// </summary>
    /// <param name="feed">Loaded RSS XML document.</param>
    /// <param name="lookup">Topic lookup for normalization.</param>
    /// <param name="settings">Cleaner, validation, and positivity settings.</param>
    /// <param name="source">Snapshot of the source row (feed URL, defaults, thumbnails).</param>
    /// <param name="cancellationToken">Token observed between items.</param>
    /// <returns>Accepted items and a count of items skipped due to errors or validation.</returns>
    FeedProcessingResult ProcessFeed(
        XDocument feed,
        TopicLookup lookup,
        IngestionSettingsSnapshot settings,
        IngestionSourceSnapshot source,
        CancellationToken cancellationToken = default);
}
