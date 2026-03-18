using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace PositiveNews.Infrastructure.Services;

public class FeedReader : IFeedReader
{
    private readonly IHttpClientFactory _httpClientFactory; // registered in Infrastructure.DependencyInjection (services.AddHttpClient())
    private readonly ILogger<FeedReader> _logger;

    public FeedReader(IHttpClientFactory httpClientFactory, ILogger<FeedReader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    async Task<XDocument> IFeedReader.ReadFeedAsync(string feedUrl, CancellationToken cancellationToken)
    {
        var feed = new XDocument();
        try
        {
            _logger.LogInformation("Fetching RSS feed from {FeedUrl}", feedUrl);

            var httpClient = _httpClientFactory.CreateClient("RssFeedClient");
            using var response = await httpClient.GetAsync(feedUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            feed = XDocument.Load(stream);
            return feed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read feed from {FeedUrl}.", feedUrl);
            return feed;
        }
    }
}