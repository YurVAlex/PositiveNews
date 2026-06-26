using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;

namespace PositiveNews.Application.Queries.Admin;

/// <summary>
/// Loads a single article for administrative moderation.
/// </summary>
/// <param name="ArticleId">Article identifier.</param>
public sealed record GetAdminArticleDetailQuery(long ArticleId) : IRequest<Result<ArticleAdminDetailDto>>;
