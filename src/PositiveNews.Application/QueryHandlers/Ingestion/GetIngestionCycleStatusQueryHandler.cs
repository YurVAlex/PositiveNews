using MediatR;
using PositiveNews.Application.Abstractions.Ingestion;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

/// <summary>
/// Reads ingestion cycle coordinator state for the admin panel.
/// </summary>
public sealed class GetIngestionCycleStatusQueryHandler(IIngestionCycleCoordinator coordinator)
    : IRequestHandler<GetIngestionCycleStatusQuery, IngestionCycleStatusDto>
{
    /// <inheritdoc />
    public Task<IngestionCycleStatusDto> Handle(
        GetIngestionCycleStatusQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new IngestionCycleStatusDto
        {
            IsRunning = coordinator.IsRunning,
            NextRunAtUtc = coordinator.NextRunAtUtc
        });
    }
}
