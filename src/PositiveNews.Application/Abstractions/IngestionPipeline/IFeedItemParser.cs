using PositiveNews.Application.DTOs.Ingestion;
using System.Xml.Linq;

namespace PositiveNews.Application.Abstractions.IngestionPipeline;

/// <summary>
/// Parses a single RSS <c>item</c> element into the application's feed DTO.
/// </summary>
public interface IFeedItemParser
{
    /// <summary>
    /// Maps XML fields (title, link, pubDate, etc.) onto <see cref="RssFeedItemDto"/>.
    /// </summary>
    /// <param name="itemElement">The RSS item element.</param>
    /// <returns>A new DTO representing the item.</returns>
    RssFeedItemDto Parse(XElement itemElement);
}
