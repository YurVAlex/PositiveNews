using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using System.Xml.Linq;

namespace PositiveNews.Infrastructure.Services;

public class FeedProcessor : IFeedProcessor
{
    private readonly ILogger<FeedProcessor> _logger;
    private readonly IFeedItemValidator _validator;
    private readonly IFeedItemParser _parser;
    private readonly IFeedItemCleaner _cleaner;
    private readonly IFeedItemEnricher _enricher;
    private readonly IImgTagExtractor _imgTagExtractor;
    private readonly IPositivityAnalyzer _analyzer;

    public FeedProcessor(
       IFeedItemValidator validator,
       IFeedItemParser parser,
       IFeedItemCleaner cleaner,
       IFeedItemEnricher enricher,
       ILogger<FeedProcessor> logger,
       IImgTagExtractor imgTagExtractor,
       IPositivityAnalyzer analyzer)
    {
        _validator = validator;
        _parser = parser;
        _cleaner = cleaner;
        _enricher = enricher;
        _imgTagExtractor = imgTagExtractor;
        _logger = logger;
        _analyzer = analyzer;
    }

    public FeedProcessingResult ProcessFeed(string feedUrl, XDocument feed, TopicLookup lookup, 
        CancellationToken cancellationToken = default)
    {
        var dtoItems = new List<RssFeedItemDto>();
        var invalidCount = 0;

        _logger.LogInformation("Processing RSS feed from {FeedUrl}", feedUrl);

        var feedItems = feed.Descendants("item").ToList();
        _logger.LogInformation("Found {Count} items in RSS feed.", feedItems.Count);

        foreach (var feedItem in feedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (!TryProcessFeedItem(feedUrl, feedItem, lookup, out var dtoItem))
                {
                    invalidCount++;
                    continue;
                }

                dtoItems.Add(dtoItem);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing RSS feed item");
                invalidCount++;
            }

            _logger.LogInformation("Feed item No.{Count} has been successfully processed.", dtoItems.Count);
        }

        return new FeedProcessingResult(dtoItems, invalidCount);
    }
    private bool TryProcessFeedItem(string feedUrl, XElement feedItem, TopicLookup lookup, out RssFeedItemDto dtoItem)
    {
        dtoItem = _parser.Parse(feedItem);

        var rawContentNode = ParseHtmlNode(dtoItem.ContentRaw);
        if (!_validator.IsValid(dtoItem, rawContentNode))
        {
            _logger.LogWarning("Skipping invalid feed item.");
            return false;
        }

        _cleaner.Clean(dtoItem, lookup);

        _enricher.EnrichTopics(feedUrl, dtoItem, lookup);

        if (string.IsNullOrWhiteSpace(dtoItem.ContentRaw))
            return false;

        var cleanedContentNode = ParseHtmlNode(dtoItem.ContentRaw);
        var descriptionNode = ParseHtmlNode(dtoItem.Description);

        dtoItem.ImageTag = _imgTagExtractor.ExtractImgTag(feedItem, feedUrl, cleanedContentNode, descriptionNode);
        dtoItem.ContentRaw = _enricher.AddHeroImage(dtoItem.ContentRaw, dtoItem.ImageTag, cleanedContentNode);
        cleanedContentNode = ParseHtmlNode(dtoItem.ContentRaw);

        dtoItem.ContentClean = _cleaner.StripInnerHtmlWords(dtoItem.ContentRaw, cleanedContentNode)
            ?? _cleaner.StripInnerHtmlWords(dtoItem.Description, descriptionNode);
        dtoItem.PositivityScore = _analyzer.AnalyzeSentiment(dtoItem.ContentClean);
        return true;
    }

    private static HtmlNode? ParseHtmlNode(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode;
    }

}