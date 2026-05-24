using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

/// <summary>
/// Loads recent ingestion runs for admin display.
/// </summary>
public sealed class GetIngestionRunsQueryHandler(IIngestionRunReadRepository repository)
    : IRequestHandler<GetIngestionRunsQuery, IReadOnlyList<IngestionRunListItemDto>>
{
    /// <inheritdoc />
    public Task<IReadOnlyList<IngestionRunListItemDto>> Handle(
        GetIngestionRunsQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetLatestAsync(request.Limit, cancellationToken);
    }
}
