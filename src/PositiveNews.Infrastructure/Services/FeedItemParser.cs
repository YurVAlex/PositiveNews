using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Infrastructure.Services;
using PositiveNews.Application.Interfaces;
using System.Xml.Linq;

public class FeedItemParser : IFeedItemParser
{
    private readonly ILogger<FeedReader> _logger;
    
    private static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace ContentNs = "http://purl.org/rss/1.0/modules/content/";

    public FeedItemParser(ILogger<FeedReader> logger, IImgTagExtractor imgTagextractor)
    {
        _logger = logger;
    }
    public RssFeedItemDto Parse(XElement itemElement)
    {
        return new RssFeedItemDto
        {
            Title = itemElement.Element("title")!.Value,              // "!" is because validator guarantees it exists.
            Link = itemElement.Element("link")!.Value,   
            Description = itemElement.Element("description")!.Value,
            ContentRaw = itemElement.Element(ContentNs + "encoded")!.Value,
            Author = itemElement.Element(DcNs + "creator")?.Value,    
            PublishedDate = ParseDate(itemElement),
            Topics = ExtractCategories(itemElement),
            ExternalId = itemElement.Element("guid")?.Value           //TODO: Add cleaner/cutter and regex conductor before parsing. Like in methods bellow
        };
    }

    /// <summary>
    /// Extracts all category tags as topics
    /// Returns list of category values, or ["Default"] if none found
    /// </summary>
    private List<string> ExtractCategories(XElement itemElement)
    {
        var categories = itemElement
            .Elements("category")
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .ToList();

        // If no categories found, return default
        if (categories == null || categories.Count == 0)
        {
            categories = ["Default"];
            _logger.LogDebug("No categories found, using default");
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