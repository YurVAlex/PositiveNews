using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Comments;

namespace PositiveNews.Application.Commands.Comments;

/// <summary>
/// Creates a new top-level comment on an article.
/// </summary>
/// <param name="ArticleId">Article primary key.</param>
/// <param name="UserId">Authenticated author user id.</param>
/// <param name="Content">Comment body text.</param>
public sealed record AddArticleCommentCommand(
    long ArticleId,
    long UserId,
    string Content) : IRequest<Result<CommentCreatedDto>>;
