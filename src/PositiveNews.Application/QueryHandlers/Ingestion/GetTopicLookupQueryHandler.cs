using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

/// <summary>
/// Builds the ingestion topic lookup by loading all topics and projecting indexes.
/// </summary>
/// <param name="topicReadRepository">Reads topic snapshots from persistence.</param>
/// <param name="topicLookupBuilder">Constructs lookup dictionaries from snapshots.</param>
public sealed class GetTopicLookupQueryHandler(
    ITopicReadRepository topicReadRepository,
    ITopicLookupBuilder topicLookupBuilder)
    : IRequestHandler<GetTopicLookupQuery, TopicLookup>
{
    /// <summary>
    /// Loads every topic row and builds the normalized lookup structure.
    /// </summary>
    /// <param name="request">Marker query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Immutable lookup for ingestion pipelines.</returns>
    public async Task<TopicLookup> Handle(GetTopicLookupQuery request, CancellationToken cancellationToken)
    {
        var snapshots = await topicReadRepository.GetAllTopicSnapshotsAsync(cancellationToken);
        return topicLookupBuilder.Build(snapshots);
    }
}
