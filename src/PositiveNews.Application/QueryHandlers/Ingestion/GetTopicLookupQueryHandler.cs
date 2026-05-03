using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

public sealed class GetTopicLookupQueryHandler(
    ITopicReadRepository topicReadRepository,
    ITopicLookupBuilder topicLookupBuilder)
    : IRequestHandler<GetTopicLookupQuery, TopicLookup>
{
    public async Task<TopicLookup> Handle(GetTopicLookupQuery request, CancellationToken cancellationToken)
    {
        var snapshots = await topicReadRepository.GetAllTopicSnapshotsAsync(cancellationToken);
        return topicLookupBuilder.Build(snapshots);
    }
}
