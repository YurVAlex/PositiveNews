using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Infrastructure.Services;
using System.Net;
using System.Text;
using System.Xml.Linq;

public class FeedItemParser : IFeedItemParser
{
    private readonly ILogger<FeedReader> _logger;
    
    private static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";

    public FeedItemParser(ILogger<FeedReader> logger)
    {
        _logger = logger;
    }
    public RssFeedItemDto Parse(XElement itemElement)
    {
        // "!" is because validator guarantees it exists.

        return new RssFeedItemDto
        {
            Title = itemElement.Element("title")!.Value,  
            Link = itemElement.Element("link")!.Value,   
            Description = itemElement.Element("description")!.Value,
            ContentRaw = itemElement.Element(ContentNs + "encoded")!.Value,
            Author = itemElement.Element(DcNs + "creator")?.Value,    
            PublishedDate = ParseDate(itemElement),
            ImageTag = ExtractThumbnailImgTag(itemElement),
            Topics = ExtractCategories(itemElement),
            ExternalId = itemElement.Element("guid")?.Value           //TODO: Add cleaner/cutter and regex conductor before parsing. Like in methods bellow
        };
    }

    /// <summary>
    /// Extracts image URL:
    /// </summary>
    public static string? ExtractThumbnailImgTag(XElement itemElement)
    {
        // 1. media:thumbnail (direct child or nested inside media:content)
        var thumbnail =
            itemElement.Element(MediaNs + "thumbnail") ??
            itemElement.Element(MediaNs + "content")?.Element(MediaNs + "thumbnail");

        if (thumbnail != null)
        {
            return BuildImgTag(
                src: thumbnail.Attribute("url")?.Value,
                width: thumbnail.Attribute("width")?.Value,
                height: thumbnail.Attribute("height")?.Value,
                alt: thumbnail.Attribute("alt")?.Value ?? "Article thumbnail"
            );
        }

        // 2. First <img> inside <description>
        var descriptionHtml = itemElement.Element("description")?.Value;
        if (!string.IsNullOrWhiteSpace(descriptionHtml))
        {
            var imgFromDesc = ExtractImgFromHtml(descriptionHtml);
            if (imgFromDesc != null)
                return imgFromDesc;
        }

        // 3. media:content (full image)
        var mediaContent = itemElement.Element(MediaNs + "content");
        if (mediaContent != null)
        {
            return BuildImgTag(
                src: mediaContent.Attribute("url")?.Value,
                width: mediaContent.Attribute("width")?.Value,
                height: mediaContent.Attribute("height")?.Value,
                alt: mediaContent.Attribute("alt")?.Value ?? "Article image"
            );
        }

        // 4. First <img> inside <content:encoded> (only reached if needed)
        var encodedHtml = itemElement.Element(ContentNs + "encoded")?.Value;
        if (!string.IsNullOrWhiteSpace(encodedHtml))
        {
            var imgFromEncoded = ExtractImgFromHtml(encodedHtml);
            if (imgFromEncoded != null)
                return imgFromEncoded;
        }

        return null;
    }

    private static string? ExtractImgFromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var img = doc.DocumentNode.SelectSingleNode("//img");
        if (img == null)
            return null;

        return BuildImgTag(
            src: img.GetAttributeValue("src", ""),
            width: img.GetAttributeValue("width", "800"),
            height: img.GetAttributeValue("height", "600"),
            alt: img.GetAttributeValue("alt", "Article image")
        );
    }

    private static string? BuildImgTag(
    string? src = "",
    string? width = "800",
    string? height = "600",
    string? alt = "Article image")
    {
        if (string.IsNullOrWhiteSpace(src))
            return null;

        var sb = new StringBuilder();
        sb.Append($"<img src=\"{WebUtility.HtmlEncode(src)}\" ");
        sb.Append($"class=\"img-fluid w-100 rounded mb-3\" ");
        sb.Append($"width = \"{width}\" height = \"{height}\" ");
        sb.Append($"alt=\"{WebUtility.HtmlEncode(alt)}\" ");

        sb.Append("style=\"object-fit: cover;\" ");  

        sb.Append("/>");
        return sb.ToString();
    }

    /// <summary>
    /// Validates that URL is a proper image URL
    /// </summary>
    private bool IsValidImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Must start with http/https
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            return false;

        // Must have image extension
        var lowerUrl = url.ToLower();
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        return validExtensions.Any(ext => lowerUrl.Contains(ext));
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