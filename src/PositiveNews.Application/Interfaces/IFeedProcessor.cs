using PositiveNews.Application.DTOs;
using System.Xml.Linq;

namespace PositiveNews.Application.Interfaces;

public interface IFeedProcessor
{
    IReadOnlyList<RssFeedItemDto> ProcessFeed(string feedUrl, XDocument feed, TopicLookup lookup, out int invalidCount);
}