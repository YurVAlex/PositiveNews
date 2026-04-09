using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IFeedItemCleaner
{
    void Clean(RssFeedItemDto dto);
    void CleanTopics(RssFeedItemDto dto, TopicLookup lookup);
    public string? StripInnerHtmlWords(string htmlContent);
}