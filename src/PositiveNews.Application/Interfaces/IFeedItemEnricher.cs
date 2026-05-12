using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

/// <summary>
/// Enriches feed items with inferred topics and hero imagery after cleaning.
/// </summary>
public interface IFeedItemEnricher
{
    /// <summary>
    /// Applies default topics and slug-based inference using settings and the topic lookup.
    /// </summary>
    /// <param name="feedUrl">RSS feed URL (used for source-specific rules).</param>
    /// <param name="dto">Item being enriched.</param>
    /// <param name="lookup">Topic lookup built from the database.</param>
    /// <param name="settings">Ingestion settings snapshot including per-source rules.</param>
    /// <returns>The item with updated topic list.</returns>
    RssFeedItemDto EnrichTopics(string feedUrl, RssFeedItemDto dto, TopicLookup lookup,
        IngestionSettingsSnapshot settings);

    /// <summary>
    /// Ensures a prominent image is present when the extractor found a tag or fallback is needed.
    /// </summary>
    /// <param name="dto">Item to update.</param>
    /// <param name="imageTag">HTML img tag or embedding snippet.</param>
    /// <param name="contentNode">Parsed content root for layout heuristics.</param>
    /// <returns>The item with image metadata applied.</returns>
    RssFeedItemDto AddHeroImage(RssFeedItemDto dto, string? imageTag, HtmlNode? contentNode);
}
