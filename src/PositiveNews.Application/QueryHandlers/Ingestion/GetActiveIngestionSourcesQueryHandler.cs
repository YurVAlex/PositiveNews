using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

public sealed class GetActiveIngestionSourcesQueryHandler(ISourceReadRepository sourceReadRepository)
    : IRequestHandler<GetActiveIngestionSourcesQuery, IReadOnlyList<IngestionSourceSnapshot>>
{
    public Task<IReadOnlyList<IngestionSourceSnapshot>> Handle(
        GetActiveIngestionSourcesQuery request,
        CancellationToken cancellationToken)
    {
        return sourceReadRepository.GetActiveIngestionSourcesAsync(cancellationToken);
    }
}
