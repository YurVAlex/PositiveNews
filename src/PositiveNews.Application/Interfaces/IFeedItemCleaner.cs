using HtmlAgilityPack;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IFeedItemCleaner
{
    void Clean(RssFeedItemDto dto, TopicLookup lookup);
    void CleanTopics(RssFeedItemDto dto, TopicLookup lookup);
    string? StripInnerHtmlWords(string? htmlContent, HtmlNode? htmlNode = null);
}