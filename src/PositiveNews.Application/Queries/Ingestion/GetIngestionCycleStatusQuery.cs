using MediatR;
using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Queries.Ingestion;

/// <summary>Returns current ingestion cycle scheduler state for the admin panel.</summary>
public sealed record GetIngestionCycleStatusQuery : IRequest<IngestionCycleStatusDto>;
