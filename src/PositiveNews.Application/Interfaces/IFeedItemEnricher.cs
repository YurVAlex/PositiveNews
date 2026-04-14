using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IFeedItemEnricher
{
    RssFeedItemDto EnrichTopics(string feedUrl, RssFeedItemDto dto, TopicLookup lookup);
    string AddHeroImage(string contentRaw, string? imageTag, HtmlNode? contentNode);
}
