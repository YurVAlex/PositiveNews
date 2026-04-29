using PositiveNews.Application.DTOs;
using System.Xml.Linq;

namespace PositiveNews.Application.Interfaces;

public interface IFeedProcessor
{
    FeedProcessingResult ProcessFeed(
        string feedUrl,
        XDocument feed,
        TopicLookup lookup,
        IngestionSettingsSnapshot settings,
        IngestionSourceSnapshot source,
        CancellationToken cancellationToken = default);
}
