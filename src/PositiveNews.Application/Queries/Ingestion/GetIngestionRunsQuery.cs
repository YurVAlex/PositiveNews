using MediatR;
using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Queries.Ingestion;

/// <summary>Returns recent ingestion runs for the admin panel.</summary>
public sealed record GetIngestionRunsQuery(int Limit = 200) : IRequest<IReadOnlyList<IngestionRunListItemDto>>;
