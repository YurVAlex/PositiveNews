namespace PositiveNews.Application.Services.Ingestion;

using PositiveNews.Application.DTOs;

public interface ITopicLookupBuilder
{
    TopicLookup Build(IReadOnlyList<TopicSnapshot> topics);
}
