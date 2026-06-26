using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Comments;

namespace PositiveNews.Application.Queries.Comments;

/// <summary>
/// Loads active top-level comments for a single article.
/// </summary>
/// <param name="ArticleId">Article primary key.</param>
public sealed record GetArticleCommentsQuery(long ArticleId) : IRequest<Result<IReadOnlyList<CommentListItemDto>>>;
