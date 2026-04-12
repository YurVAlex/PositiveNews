using MediatR;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Queries.Ingestion;

public sealed record GetActiveIngestionSourcesQuery : IRequest<IReadOnlyList<IngestionSourceSnapshot>>;
