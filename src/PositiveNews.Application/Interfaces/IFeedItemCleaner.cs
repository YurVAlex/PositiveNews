using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

/// <summary>
/// Normalizes HTML content, strips boilerplate, and aligns topic names with the taxonomy.
/// </summary>
public interface IFeedItemCleaner
{
    /// <summary>
    /// Applies cleaner rules and topic normalization to a parsed feed item.
    /// </summary>
    /// <param name="dto">Mutable RSS item DTO.</param>
    /// <param name="lookup">Topic lookup built from the database.</param>
    /// <param name="rules">Configurable removal and allow-list rules.</param>
    /// <param name="rawContentNode">Optional parsed HTML root for the raw content.</param>
    /// <returns>The cleaned DTO (may be the same instance with updated fields).</returns>
    RssFeedItemDto Clean(RssFeedItemDto dto, TopicLookup lookup, CleanerRules rules, HtmlNode? rawContentNode);

    /// <summary>
    /// Normalizes raw topic strings from the feed to known topic names.
    /// </summary>
    /// <param name="topics">Raw topic labels.</param>
    /// <param name="lookup">Topic lookup built from the database.</param>
    /// <returns>Filtered and normalized topic names.</returns>
    IReadOnlyList<string> CleanTopics(IReadOnlyList<string> topics, TopicLookup lookup);

    /// <summary>
    /// Removes configured inner fragments from HTML and returns plain text for analysis.
    /// </summary>
    /// <param name="htmlContent">HTML string content.</param>
    /// <param name="htmlNode">Optional pre-parsed HTML node tree.</param>
    /// <returns>Plain text suitable for sentiment scoring, or <see langword="null"/>.</returns>
    string? StripInnerHtmlWords(string? htmlContent, HtmlNode? htmlNode = null);
}
