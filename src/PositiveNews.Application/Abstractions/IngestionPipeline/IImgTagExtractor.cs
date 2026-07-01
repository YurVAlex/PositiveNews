using HtmlAgilityPack;
using System.Xml.Linq;

namespace PositiveNews.Application.Abstractions.IngestionPipeline;

/// <summary>
/// Extracts a representative <c>img</c> tag or thumbnail markup from RSS fields.
/// </summary>
public interface IImgTagExtractor
{
    /// <summary>
    /// Chooses the best image markup from item XML and parsed HTML, including optional defaults.
    /// </summary>
    /// <param name="itemElement">Raw RSS item element.</param>
    /// <param name="feedUrl">Feed URL for resolving relative links.</param>
    /// <param name="contentNode">Parsed main content HTML.</param>
    /// <param name="descriptionNode">Parsed description HTML.</param>
    /// <param name="defaultThumbnailHtml">Optional HTML snippet from source configuration.</param>
    /// <returns>Image tag HTML or <see langword="null"/>.</returns>
    string? ExtractImgTag(XElement itemElement, string feedUrl, HtmlNode? contentNode,
        HtmlNode? descriptionNode, string? defaultThumbnailHtml);
}
