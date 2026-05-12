using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using System.Xml.Linq;

namespace PositiveNews.Application.Services.Ingestion;

/// <summary>
/// Default RSS processing pipeline: parse, validate, clean, enrich topics, score positivity, and extract imagery.
/// </summary>
public class FeedProcessingPipeline : IFeedProcessor
{
    private readonly ILogger<FeedProcessingPipeline> _logger;
    private readonly IFeedItemValidator _validator;
    private readonly IFeedItemParser _parser;
    private readonly IFeedItemCleaner _cleaner;
    private readonly IFeedItemEnricher _enricher;
    private readonly IImgTagExtractor _imgTagExtractor;
    private readonly IPositivityAnalyzer _analyzer;

    /// <summary>
    /// Initializes pipeline stages used for each RSS item.
    /// </summary>
    /// <param name="validator">Early rejection rules.</param>
    /// <param name="parser">Maps RSS XML elements to DTOs.</param>
    /// <param name="cleaner">Sanitizes HTML and topics.</param>
    /// <param name="enricher">Applies topic inference and hero images.</param>
    /// <param name="logger">Diagnostic logging.</param>
    /// <param name="imgTagExtractor">Chooses representative images.</param>
    /// <param name="analyzer">Scores cleaned plain text.</param>
    public FeedProcessingPipeline(
       IFeedItemValidator validator,
       IFeedItemParser parser,
       IFeedItemCleaner cleaner,
       IFeedItemEnricher enricher,
       ILogger<FeedProcessingPipeline> logger,
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

    /// <summary>
    /// Walks every RSS item element, produces enriched DTOs, and counts skipped invalid entries.
    /// </summary>
    /// <param name="feedUrl">Feed URL for logging and rule selection.</param>
    /// <param name="feed">Loaded RSS document.</param>
    /// <param name="lookup">Topic normalization indexes.</param>
    /// <param name="settings">Cleaner, validation, and positivity configuration.</param>
    /// <param name="source">Source snapshot with defaults such as thumbnails.</param>
    /// <param name="cancellationToken">Cancellation observed per item.</param>
    /// <returns>Accepted items and invalid-item tally.</returns>
    public FeedProcessingResult ProcessFeed(
        string feedUrl, XDocument feed, TopicLookup lookup,
        IngestionSettingsSnapshot settings, IngestionSourceSnapshot source,
        CancellationToken cancellationToken = default)
    {
        var dtoItems = new List<RssFeedItemDto>();
        var invalidCount = 0;

        _logger.LogInformation("Processing RSS feed from {FeedUrl}", feedUrl);

        foreach (var feedItem in feed.Descendants("item"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (!TryProcessFeedItem(feedUrl, feedItem, lookup, settings, source, out var dtoItem))
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

            _logger.LogDebug("Feed item No.{Count} has been successfully processed.", dtoItems.Count);
        }

        return new FeedProcessingResult(dtoItems, invalidCount);
    }

    /// <summary>
    /// Parses one item, validates, cleans, enriches topics, scores sentiment, extracts imagery, and applies hero image rules.
    /// </summary>
    private bool TryProcessFeedItem(
        string feedUrl, XElement feedItem, TopicLookup lookup,
        IngestionSettingsSnapshot settings, IngestionSourceSnapshot source,
        out RssFeedItemDto dtoItem)
    {
        dtoItem = _parser.Parse(feedItem);

        var rawContentNode = ParseHtmlNode(dtoItem.ContentRaw);
        if (!_validator.IsValid(dtoItem, settings.FeedItemValidationRules, rawContentNode))
        {
            _logger.LogWarning("Skipping invalid feed item.");
            return false;
        }

        dtoItem = _cleaner.Clean(dtoItem, lookup, settings.CleanerRules, rawContentNode);

        if (string.IsNullOrWhiteSpace(dtoItem.ContentRaw))
            return false;

        dtoItem = _enricher.EnrichTopics(feedUrl, dtoItem, lookup, settings);

        var cleanedContentNode = ParseHtmlNode(dtoItem.ContentRaw);
        var descriptionNode = ParseHtmlNode(dtoItem.Description);

        var contentClean = _cleaner.StripInnerHtmlWords(dtoItem.ContentRaw, cleanedContentNode)
            ?? _cleaner.StripInnerHtmlWords(dtoItem.Description, descriptionNode);

        var positivityScore = _analyzer.AnalyzeSentiment(contentClean, settings.PositivityAnalizerKeyPhrases);
        var imageTag = _imgTagExtractor.ExtractImgTag(feedItem, feedUrl, contentNode: cleanedContentNode,
            descriptionNode: descriptionNode, defaultThumbnailHtml: source.DefaultThumbnailHtml);

        dtoItem = dtoItem with
        {
            ContentClean = contentClean,
            PositivityScore = positivityScore,
            ImageTag = imageTag
        };

        dtoItem = _enricher.AddHeroImage(dtoItem, imageTag, cleanedContentNode);

        return true;
    }

    /// <summary>
    /// Parses minimal HTML into an <see cref="HtmlNode"/> document root when content exists.
    /// </summary>
    private static HtmlNode? ParseHtmlNode(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode;
    }
}
