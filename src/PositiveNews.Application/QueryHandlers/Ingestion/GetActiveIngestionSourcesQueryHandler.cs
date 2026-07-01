using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

/// <summary>
/// Lists RSS sources that should be polled during ingestion cycles.
/// </summary>
/// <param name="sourceReadRepository">Reads enabled sources with feed URLs.</param>
public sealed class GetActiveIngestionSourcesQueryHandler(ISourceReadRepository sourceReadRepository)
    : IRequestHandler<GetActiveIngestionSourcesQuery, IReadOnlyList<IngestionSourceSnapshot>>
{
    /// <summary>
    /// Returns snapshots for every active ingestion source.
    /// </summary>
    /// <param name="request">Marker query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Source snapshots for orchestration.</returns>
    public Task<IReadOnlyList<IngestionSourceSnapshot>> Handle(
        GetActiveIngestionSourcesQuery request,
        CancellationToken cancellationToken)
    {
        return sourceReadRepository.GetActiveIngestionSourcesAsync(cancellationToken);
    }
}
