using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Fetches and parses an RSS/Atom feed using System.ServiceModel.Syndication.
/// This is the only class that makes HTTP calls to external sources.
/// </summary>
public class RssFeedReader : IRssFeedReader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RssFeedReader> _logger;

    public RssFeedReader(IHttpClientFactory httpClientFactory, ILogger<RssFeedReader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RssFeedItemDto>> ReadFeedAsync(
        string feedUrl, CancellationToken cancellationToken = default)
    {
        var items = new List<RssFeedItemDto>();

        try
        {
            var httpClient = _httpClientFactory.CreateClient("RssFeedClient");
            using var response = await httpClient.GetAsync(feedUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            var feed = SyndicationFeed.Load(xmlReader);
            if (feed is null)
            {
                _logger.LogWarning("Feed at {FeedUrl} returned null after parsing.", feedUrl);
                return items;
            }

            foreach (var syndicationItem in feed.Items)
            {
                var dto = new RssFeedItemDto
                {
                    Title = syndicationItem.Title?.Text ?? "(No Title)",
                    Link = syndicationItem.Links.FirstOrDefault()?.Uri?.AbsoluteUri ?? string.Empty,
                    Description = syndicationItem.Summary?.Text,
                    Author = syndicationItem.Authors.FirstOrDefault()?.Name
                              ?? syndicationItem.Authors.FirstOrDefault()?.Email,
                    PublishedDate = syndicationItem.PublishDate != DateTimeOffset.MinValue
                        ? syndicationItem.PublishDate.UtcDateTime
                        : syndicationItem.LastUpdatedTime != DateTimeOffset.MinValue
                            ? syndicationItem.LastUpdatedTime.UtcDateTime
                            : null,
                    ExternalId = syndicationItem.Id ?? syndicationItem.Links.FirstOrDefault()?.Uri?.AbsoluteUri,
                    ImageUrl = ExtractImageUrl(syndicationItem)
                };

                if (!string.IsNullOrWhiteSpace(dto.Link))
                {
                    items.Add(dto);
                }
            }

            _logger.LogInformation("Parsed {Count} items from {FeedUrl}.", items.Count, feedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read feed from {FeedUrl}.", feedUrl);
        }

        return items;
    }

    /// <summary>
    /// Attempts to extract an image URL from common RSS extensions like media:thumbnail,
    /// media:content, or enclosure elements.
    /// </summary>
    private static string? ExtractImageUrl(SyndicationItem item)
    {
        // 1. Try media:thumbnail or media:content (common in RSS 2.0 feeds)
        foreach (var extension in item.ElementExtensions)
        {
            if (extension.OuterName is "thumbnail" or "content"
                && extension.OuterNamespace == "http://search.yahoo.com/mrss/")
            {
                var element = extension.GetObject<XmlElement>();
                var url = element?.GetAttribute("url");
                if (!string.IsNullOrWhiteSpace(url)) return url;
            }
        }

        // 2. Try enclosure (used by some feeds for images)
        foreach (var link in item.Links)
        {
            if (link.RelationshipType == "enclosure"
                && link.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true)
            {
                return link.Uri?.AbsoluteUri;
            }
        }

        return null;
    }
}