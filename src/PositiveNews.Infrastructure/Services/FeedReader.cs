using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using System.Xml.Linq;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Downloads RSS/Atom XML over HTTP using the named <c>RssFeedClient</c> HttpClient.
/// </summary>
public class FeedReader : IFeedReader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FeedReader> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedReader"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory that supplies the RSS HTTP client.</param>
    /// <param name="logger">Diagnostic logger.</param>
    public FeedReader(IHttpClientFactory httpClientFactory, ILogger<FeedReader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    async Task<XDocument> IFeedReader.ReadFeedAsync(string feedUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching RSS feed from {FeedUrl}", feedUrl);

        var httpClient = _httpClientFactory.CreateClient("RssFeedClient");
        using var response = await httpClient.GetAsync(feedUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
    }
}