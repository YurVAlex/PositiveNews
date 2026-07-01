using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Constants;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using System.Xml.Linq;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Parses a single RSS <c>item</c> element into <see cref="RssFeedItemDto"/> (title, link, content:encoded, categories, etc.).
/// </summary>
public class FeedItemParser : IFeedItemParser
{
    private readonly ILogger<FeedItemParser> _logger;
    
    private static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace ContentNs = "http://purl.org/rss/1.0/modules/content/";

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedItemParser"/> class.
    /// </summary>
    /// <param name="logger">Logger for parse diagnostics.</param>
    public FeedItemParser(ILogger<FeedItemParser> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Maps RSS fields from <paramref name="itemElement"/> into a DTO.
    /// </summary>
    /// <param name="itemElement">The XML <c>item</c> node.</param>
    /// <returns>A populated feed item DTO.</returns>
    public RssFeedItemDto Parse(XElement itemElement)
    {
        return new RssFeedItemDto
        {
            Title = itemElement.Element("title")?.Value?.Trim() ?? string.Empty,
            Link = itemElement.Element("link")?.Value?.Trim() ?? string.Empty,
            Description = itemElement.Element("description")?.Value?.Trim() ?? string.Empty,
            ContentRaw = itemElement.Element(ContentNs + "encoded")?.Value?.Trim() ?? string.Empty,
            Author = itemElement.Element(DcNs + "creator")?.Value?.Trim(),
            PublishedDate = ParseDate(itemElement),
            Topics = ExtractCategories(itemElement),
            ExternalId = itemElement.Element("guid")?.Value?.Trim()
        };
    }
    private List<string> ExtractCategories(XElement itemElement)
    {
        var categories = itemElement
            .Elements("category")
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .ToList(); // Creates a NEW list each time

        if (categories.Count == 0)
        {
            categories = [IngestionCatalogConstants.DefaultTopicName];
        }

        _logger.LogDebug("Extracted categories: {Categories}", string.Join(", ", categories));
        return categories;
    }

    private static DateTime ParseDate(XElement item)
    {
        var dateStr = item.Element("pubDate")?.Value?.Trim();

        if (!string.IsNullOrWhiteSpace(dateStr))
        {
            // Common RSS date formats
            string[] formats = new[]
            {
            "ddd, dd MMM yyyy HH:mm:ss zzz",
            "ddd, dd MMM yyyy HH:mm:ss K",
            "ddd, dd MMM yyyy HH:mm:ss",
            "yyyy-MM-ddTHH:mm:sszzz",
            "yyyy-MM-ddTHH:mm:ssK",
            "yyyy-MM-ddTHH:mm:ss"
            };

            if (DateTime.TryParseExact(dateStr, formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
            // Try standard parse as fallback
            if (DateTime.TryParse(dateStr, out result))
            {
                return result;
            }
        }
        return DateTime.UtcNow;
    }
}
