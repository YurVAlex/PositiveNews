using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

/// <summary>
/// Rejects feed items that fail minimum quality or authorship rules before expensive processing.
/// </summary>
public interface IFeedItemValidator
{
    /// <summary>
    /// Returns whether the item passes validation given rules and optional parsed HTML.
    /// </summary>
    /// <param name="item">Parsed RSS item.</param>
    /// <param name="rules">Configurable validation rules.</param>
    /// <param name="contentNode">Parsed HTML root for the main content when available.</param>
    /// <returns><see langword="true"/> when the item should proceed through the pipeline.</returns>
    bool IsValid(RssFeedItemDto item, FeedItemValidationRules rules, HtmlNode? contentNode);
}
