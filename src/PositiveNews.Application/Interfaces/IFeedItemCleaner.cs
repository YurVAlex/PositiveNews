using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IFeedItemCleaner
{
    RssFeedItemDto Clean(RssFeedItemDto dto, TopicLookup lookup, CleanerRules rules, HtmlNode? rawContentNode);
    IReadOnlyList<string> CleanTopics(IReadOnlyList<string> topics, TopicLookup lookup);
    string? StripInnerHtmlWords(string? htmlContent, HtmlNode? htmlNode = null);
}
