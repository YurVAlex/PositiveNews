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
    private readonly IRssItemElementValidator _validator; 
    private readonly IRssItemParser _parser;

    public RssFeedReader(
    IHttpClientFactory httpClientFactory,
    ILogger<RssFeedReader> logger,
    IRssItemElementValidator validator,
    IRssItemParser parser
    )
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _validator = validator;
        _parser = parser;
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
                    if (!_validator.IsValid(itemElement))
                        continue;

                    var dto = _parser.Parse(itemElement);

                    items.Add(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing RSS item");
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
   
}