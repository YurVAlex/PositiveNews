using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface ITopicNormalizer
{
    IReadOnlyList<string> NormalizeTopics(IReadOnlyList<string> rawTopics, TopicLookup lookup);
}
