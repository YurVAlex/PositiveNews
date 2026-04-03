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

    public FeedProcessor(IFeedItemValidator validator,
                         IFeedItemParser parser,
                         IFeedItemCleaner cleaner,
                         ILogger<FeedProcessor> loger, 
                         IImgTagExtractor imgTagExtractor)
    {
        _validator = validator;
        _parser = parser;
        _cleaner = cleaner;
        _imgTagExtractor = imgTagExtractor;
        _logger = loger;
    }

    public IReadOnlyList<RssFeedItemDto> ProcessFeed(string feedUrl, XDocument feed, out int invalidCount)
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

                    _cleaner.Clean(dtoItem);

                    dtoItem.ImageTag = _imgTagExtractor.ExtractImgTag(feedItem, dtoItem.ContentClean);

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
}
