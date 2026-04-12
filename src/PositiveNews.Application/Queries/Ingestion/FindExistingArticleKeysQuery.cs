using MediatR;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Queries.Ingestion;

public sealed record FindExistingArticleKeysQuery(
    IReadOnlyCollection<string?> ExternalIds,
    IReadOnlyCollection<string> Urls,
    IReadOnlyCollection<string> Titles) : IRequest<ExistingArticleKeys>;
