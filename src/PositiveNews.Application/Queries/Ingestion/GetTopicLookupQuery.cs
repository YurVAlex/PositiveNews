using MediatR;

using PositiveNews.Application.DTOs;



namespace PositiveNews.Application.Queries.Ingestion;



/// <summary>

/// Builds the in-memory topic lookup from all topics currently stored.

/// </summary>

public sealed record GetTopicLookupQuery : IRequest<TopicLookup>;

