using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Abstractions.IngestionPipeline;

/// <summary>
/// Maps arbitrary topic strings from feeds onto canonical topic names using <see cref="TopicLookup"/>.
/// </summary>
public interface ITopicNormalizer
{
    /// <summary>
    /// Normalizes each raw topic against known topics and aliases.
    /// </summary>
    /// <param name="rawTopics">Labels extracted from the feed.</param>
    /// <param name="lookup">Pre-built topic lookup.</param>
    /// <returns>Canonical topic names suitable for persistence.</returns>
    IReadOnlyList<string> NormalizeTopics(IReadOnlyList<string> rawTopics, TopicLookup lookup);
}
