using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IFeedItemEnricher
{
    RssFeedItemDto EnrichTopics(string feedUrl, RssFeedItemDto dto, TopicLookup lookup,
        IngestionSettingsSnapshot settings);
    RssFeedItemDto AddHeroImage(RssFeedItemDto dto, string? imageTag, HtmlNode? contentNode);
}
