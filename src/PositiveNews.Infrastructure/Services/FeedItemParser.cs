using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Infrastructure.Services;
using System.Net;
using System.Xml.Linq;

public class FeedItemParser : IFeedItemParser
{
    private readonly ILogger<FeedReader> _logger;

        public FeedItemParser(ILogger<FeedReader> logger)
    {
        _logger = logger;
    }
    public RssFeedItemDto Parse(XElement itemElement, XNamespace mediaNs, 
                                XNamespace contentNs, XNamespace dcNs)
    {
        // TODO Fix DTO
        // "!" is because validator guarantees it exists.

        return new RssFeedItemDto
        {
            Title = itemElement.Element("title")!.Value,  //TODO: Add cleaner/cutter and regex conductor before parsing. Like in methods bellow
            Link = itemElement.Element("link")!.Value,    //TODO: Add cleaner/cutter and regex conductor before parsing. Like in methods bellow
            Description = itemElement.Element("description")!.Value,
            ContentRaw = itemElement.Element(contentNs + "encoded")!.Value,
            Author = itemElement.Element(dcNs + "creator")?.Value,    //TODO: Add cleaner/cutter and regex conductor before parsing. Like in methods bellow
            PublishedDate = ParseDate(itemElement),
            ImageUrl = ExtractImageUrl(itemElement, mediaNs, contentNs),
            Topics = ExtractCategories(itemElement),
            ExternalId = itemElement.Element("guid")?.Value           //TODO: Add cleaner/cutter and regex conductor before parsing. Like in methods bellow
        };
    }

    /// <summary>
    /// Extracts image URL:
    /// </summary>
    private string? ExtractImageUrl(XElement itemElement, XNamespace mediaNs, XNamespace contentNs)
    {
        // TRY 1: media:content
        var mediaContent = itemElement.Element(mediaNs + "content");
        if (mediaContent != null)
        {
            var url = mediaContent.Attribute("url")?.Value;
            if (!string.IsNullOrWhiteSpace(url))
            {
                _logger.LogDebug("Found image URL in media:content: {Url}", url);
                return url;
            }
        }
        // TRY 2: media:thumbnail
        var mediaThumbnail = itemElement.Element(mediaNs + "thumbnail");
        if (mediaThumbnail != null)
        {
            var url = mediaThumbnail.Attribute("url")?.Value;
            if (!string.IsNullOrWhiteSpace(url))
            {
                _logger.LogDebug("Found image URL in media:thumbnail: {Url}", url);
                return url;
            }
        }
        // TRY 3: Extract from content:encoded (fallback)
        var contentElement = itemElement.Element(contentNs + "encoded");
        if (contentElement != null)
        {
            var imageUrl = ExtractImageUrlFromHtml(contentElement.Value);
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                _logger.LogDebug("Found image URL in content:encoded: {Url}", imageUrl);
                return imageUrl;
            }
        }
        return null;
    }

    /// <summary>
    /// Extracts first image URL from HTML content
    /// Looks for: img src, picture, or first jpg/png URL
    /// </summary>
    private string? ExtractImageUrlFromHtml(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
            return null;

        try
        {
            // TRY 1: Find <img> tag with src attribute
            var imgMatch = System.Text.RegularExpressions.Regex.Match(
                htmlContent,
                @"<img[^>]+src=[""']?([^""'\s>]+)[""']?",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (imgMatch.Success)
            {
                var src = imgMatch.Groups[1].Value;
                if (IsValidImageUrl(src))
                {
                    return src;
                }
            }

            // TRY 2: Find first .jpg or .png URL
            var urlMatch = System.Text.RegularExpressions.Regex.Match(
                htmlContent,
                @"https?://[^\s""'<>]+\.(jpg|jpeg|png|gif|webp)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (urlMatch.Success)
            {
                return urlMatch.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting image URL from HTML");
        }

        return null;
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

    private static DateTime? ParseDate(XElement item)
    {
        var dateStr = item.Element("pubDate")?.Value?.Trim();

        if (string.IsNullOrEmpty(dateStr)) return null;

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

        return null;
    }

   
}