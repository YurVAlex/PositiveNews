namespace PositiveNews.Application.Interfaces;

using PositiveNews.Application.DTOs;

public interface ITopicLookupBuilder
{
    TopicLookup Build(IReadOnlyList<TopicSnapshot> topics);
}
