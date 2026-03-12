using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Fetches and parses an RSS/Atom feed using System.ServiceModel.Syndication.
/// This class makes HTTP calls to external sources.
/// </summary>
public class RssFeedReader : IRssFeedReader
{
    private readonly IHttpClientFactory _httpClientFactory; // registered in Infrastructure.DependencyInjection (services.AddHttpClient())
    private readonly ILogger<RssFeedReader> _logger;

    // XML namespaces
    private static readonly XNamespace MediaNs = "http://search.yahoo.com/mrss/";
    private static readonly XNamespace ContentNs = "http://purl.org/rss/1.0/modules/content/";
    private static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";

    public RssFeedReader(IHttpClientFactory httpClientFactory, ILogger<RssFeedReader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RssFeedItemDto>> ReadFeedAsync(string feedUrl, CancellationToken cancellationToken = default)
    {
        var items = new List<RssFeedItemDto>();

        try
        {
            _logger.LogInformation("Fetching RSS feed from {FeedUrl}", feedUrl);

            var httpClient = _httpClientFactory.CreateClient("RssFeedClient");
            using var response = await httpClient.GetAsync(feedUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var doc = XDocument.Load(stream);
            var itemElements = doc.Descendants("item").ToList();

            if (itemElements is null)
            {
                _logger.LogWarning("Feed at {FeedUrl} returned null after parsing.", feedUrl);
                return items;
            }
            _logger.LogInformation("Found {Count} items in RSS feed", itemElements.Count);

            foreach (var itemElement in itemElements)
            {
                try
                {
                    var dto = ParseRssItem(itemElement);

                    // Only add if we have essential fields
                    if (!string.IsNullOrWhiteSpace(dto.Title) && !string.IsNullOrWhiteSpace(dto.Link))
                    {
                        items.Add(dto);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing individual RSS item, skipping");
                    continue;
                }
            }
            _logger.LogInformation("Successfully parsed {Count} items from {FeedUrl}", items.Count, feedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read feed from {FeedUrl}.", feedUrl);
        }
        return items;
    }

    /// <summary>
    /// Parses a single RSS item element into RssFeedItemDto
    /// </summary>
    private RssFeedItemDto ParseRssItem(XElement itemElement)
    {
        var dto = new RssFeedItemDto();

        var guidElement = itemElement.Element("guid");
        dto.ExternalId = guidElement?.Value;

        var titleElement = itemElement.Element("title");
        dto.Title = titleElement?.Value ?? "(No Title)";

        var linkElement = itemElement.Element("link");
        dto.Link = linkElement?.Value ?? string.Empty;

        var creatorElement = itemElement.Element(DcNs + "creator");
        dto.Author = creatorElement?.Value;

        var pubDateElement = itemElement.Element("pubDate");
        if (pubDateElement != null && DateTime.TryParse(pubDateElement.Value, out var pubDate))
        {
            dto.PublishedDate = pubDate;
        }

        var descriptionElement = itemElement.Element("description");
        dto.Description = descriptionElement?.Value;

        dto.ImageUrl = ExtractImageUrl(itemElement);

        var contentElement = itemElement.Element(ContentNs + "encoded");
        dto.ContentRaw = contentElement?.Value ?? string.Empty;

        dto.Topics = ExtractCategories(itemElement);

        _logger.LogDebug("Parsed item: {Title} by {Author}", dto.Title, dto.Author);

        return dto;
    }

    /// <summary>
    /// Extracts image URL:
    /// </summary>
    private string? ExtractImageUrl(XElement itemElement)
    {
        // TRY 1: media:content
        var mediaContent = itemElement.Element(MediaNs + "content");
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
        var mediaThumbnail = itemElement.Element(MediaNs + "thumbnail");
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
        var contentElement = itemElement.Element(ContentNs + "encoded");
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
}