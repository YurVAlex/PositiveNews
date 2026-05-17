using MediatR;

using PositiveNews.Application.DTOs;



namespace PositiveNews.Application.Queries.Ingestion;



/// <summary>

/// Returns snapshot rows for all enabled RSS sources eligible for polling.

/// </summary>

public sealed record GetActiveIngestionSourcesQuery : IRequest<IReadOnlyList<IngestionSourceSnapshot>>;

