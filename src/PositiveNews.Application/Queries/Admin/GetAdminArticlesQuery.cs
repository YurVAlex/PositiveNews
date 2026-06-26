using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;

namespace PositiveNews.Application.Queries.Admin;

/// <summary>
/// Searches articles for administrative moderation.
/// </summary>
public sealed record GetAdminArticlesQuery(string? SearchTerm = null)
    : IRequest<Result<IReadOnlyList<ArticleAdminItemDto>>>;
