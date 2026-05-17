using MediatR;

using PositiveNews.Application.DTOs;



namespace PositiveNews.Application.Queries.Ingestion;



/// <summary>

/// Batch-loads deduplication keys from storage for the given external IDs, URLs, and titles.

/// </summary>

/// <param name="ExternalIds">External identifiers observed in the feed batch.</param>

/// <param name="Urls">Canonical URLs from the batch.</param>

/// <param name="Titles">Titles from the batch.</param>

public sealed record FindExistingArticleKeysQuery(

    IReadOnlyCollection<string?> ExternalIds,

    IReadOnlyCollection<string> Urls,

    IReadOnlyCollection<string> Titles) : IRequest<ExistingArticleKeys>;

