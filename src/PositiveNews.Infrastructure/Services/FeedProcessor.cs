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
    private readonly IImgTagExtractor _imgTagExtractor;
    private readonly IPositivityAnalyzer _analyzer;

    public FeedProcessor(IFeedItemValidator validator,
                         IFeedItemParser parser,
                         IFeedItemCleaner cleaner,
                         ILogger<FeedProcessor> loger,
                         IImgTagExtractor imgTagExtractor,
                         IPositivityAnalyzer analyzer)
    {
        _validator = validator;
        _parser = parser;
        _cleaner = cleaner;
        _imgTagExtractor = imgTagExtractor;
        _logger = loger;
        _analyzer = analyzer;
    }

    public IReadOnlyList<RssFeedItemDto> ProcessFeed(string feedUrl, XDocument feed, TopicLookup lookup, out int invalidCount)
    {
        var dtoItems = new List<RssFeedItemDto>();
        invalidCount = 0;

        try
        {
            _logger.LogInformation("Processing RSS feed from {FeedUrl}", feedUrl);

            var feedItems = feed.Descendants("item").ToList();

            if (feedItems is null)
            {
                _logger.LogWarning("Feed at {FeedUrl} returned null after parsing.", feedUrl);
                return dtoItems;
            }
            _logger.LogInformation("Found {Count} items in RSS feed.", feedItems.Count);

            foreach (var feedItem in feedItems)
            {
                try
                {
                    if (!_validator.IsValid(feedItem))
                    {
                        _logger.LogWarning("Skipping invalid feed item.");
                        invalidCount++;
                        continue;
                    }

                    var dtoItem = _parser.Parse(feedItem);

                    if (dtoItem.Topics == null)
                        dtoItem.Topics = new List<string>();

                    _cleaner.Clean(dtoItem);
                    _cleaner.CleanTopics(dtoItem, lookup);
                    EnrichTopics(feedUrl, dtoItem, lookup);

                    if (string.IsNullOrWhiteSpace(dtoItem.ContentRaw))
                        continue;

                    dtoItem.ImageTag = _imgTagExtractor.ExtractImgTag(feedItem, dtoItem.ContentRaw, feedUrl);

                    if (!ContainsHeroImage(dtoItem.ContentRaw) &&
                        !string.IsNullOrWhiteSpace(dtoItem.ImageTag))
                    {
                        dtoItem.ContentRaw = string.Concat(dtoItem.ImageTag, dtoItem.ContentRaw);
                    }

                    dtoItem.ContentClean = _cleaner.StripInnerHtmlWords(dtoItem.ContentRaw) ??
                                           _cleaner.StripInnerHtmlWords(dtoItem.Description);

                    dtoItem.PositivityScore = _analyzer.AnalyzeSentiment(dtoItem.ContentClean);

                    dtoItems.Add(dtoItem);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing RSS feed item");
                    invalidCount++;
                }

                _logger.LogInformation("Feed item No.{Count} has been successfully processed.", dtoItems.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process RSS feed from {FeedUrl}", feedUrl);
        }

        return dtoItems;
    }

    private void EnrichTopics(string feedUrl, RssFeedItemDto dto, TopicLookup lookup)
    {
        dto.Topics ??= new List<string>();

        var result = new HashSet<string>(dto.Topics, StringComparer.OrdinalIgnoreCase);

        void Add(string name)
        {
            if (lookup.ByName.ContainsKey(name))
                result.Add(name);
        }

        // Source-specific rules
        if (feedUrl.Contains("nvidia", StringComparison.OrdinalIgnoreCase))
            Add("Technology");

        if (feedUrl.Contains("nasa", StringComparison.OrdinalIgnoreCase))
        {
            Add("Space");
            Add("Technology");
            Add("Science");
        }

        if (feedUrl.Contains("thisiscolossal", StringComparison.OrdinalIgnoreCase) ||
            feedUrl.Contains("designyoutrust", StringComparison.OrdinalIgnoreCase))
        {
            Add("Arts & Culture");
        }

        if (feedUrl.Contains("tinybuddha", StringComparison.OrdinalIgnoreCase))
            Add("Psychology");

        // Parent topic expansion using reverse lookup
        var expandedTopics = new HashSet<string>(result, StringComparer.OrdinalIgnoreCase);

        foreach (var topicName in result.ToList())
        {
            if (!lookup.ByName.TryGetValue(topicName, out var topic))
                continue;

            var slugWords = topic.Slug
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim());

            foreach (var word in slugWords)
            {
                if (lookup.ByName.TryGetValue(word, out var related))
                {
                    result.Add(related.Name);
                }
            }
        }

        foreach (var expanded in expandedTopics)
            result.Add(expanded);

        if (result.Count == 0)
            Add("Default");

        dto.Topics = result.ToList();
    }

    private bool ContainsHeroImage(string html)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var images = doc.DocumentNode.SelectNodes("//img");

            if (images == null || images.Count == 0)
                return false;

            return images.Any(img =>
            {
                var classAttr = img.GetAttributeValue("class", "");
                return classAttr.Contains("img-fluid", StringComparison.OrdinalIgnoreCase) &&
                       classAttr.Contains("w-100", StringComparison.OrdinalIgnoreCase);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking for fluid images in HTML content");
            return false;
        }
    }
}