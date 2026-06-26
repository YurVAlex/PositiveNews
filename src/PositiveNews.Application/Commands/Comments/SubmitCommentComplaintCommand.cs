using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Comments;

/// <summary>
/// Files a complaint against a comment on an article.
/// </summary>
/// <param name="ArticleId">Article primary key.</param>
/// <param name="CommentId">Comment primary key.</param>
/// <param name="UserId">Authenticated complainant user id.</param>
/// <param name="Reason">Complaint reason text.</param>
public sealed record SubmitCommentComplaintCommand(
    long ArticleId,
    long CommentId,
    long UserId,
    string Reason) : IRequest<Result>;
