using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IFeedItemCleaner
{
    RssFeedItemDto Clean(RssFeedItemDto dto, TopicLookup lookup, HtmlNode? rawContentNode);
    List<string> CleanTopics(List<string> topics, TopicLookup lookup);
    string? StripInnerHtmlWords(string? htmlContent, HtmlNode? htmlNode = null);
}