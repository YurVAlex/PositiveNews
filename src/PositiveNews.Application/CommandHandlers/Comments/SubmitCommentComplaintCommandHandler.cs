using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Commands.Comments;
using PositiveNews.Application.Common;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.CommandHandlers.Comments;

/// <summary>
/// Persists a user complaint against an active comment.
/// </summary>
public sealed class SubmitCommentComplaintCommandHandler(
    IArticleReadRepository articleReadRepository,
    ICommentReadRepository commentReadRepository,
    IComplaintWriteRepository complaintWriteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitCommentComplaintCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(
        SubmitCommentComplaintCommand request,
        CancellationToken cancellationToken)
    {
        if (!await articleReadRepository.ExistsActiveAsync(request.ArticleId, cancellationToken))
        {
            return Result.Failure(
                new Error("Article.NotFound", $"Article with id '{request.ArticleId}' was not found.", ErrorType.NotFound));
        }

        var comment = await commentReadRepository.GetActiveByIdForArticleAsync(
            request.CommentId,
            request.ArticleId,
            cancellationToken);
        if (comment is null)
        {
            return Result.Failure(
                new Error("Comment.NotFound", $"Comment with id '{request.CommentId}' was not found.", ErrorType.NotFound));
        }

        if (comment.UserId == request.UserId)
        {
            return Result.Failure(
                new Error("Comment.SelfComplaint", "You cannot file a complaint against your own comment.", ErrorType.Validation));
        }

        if (await complaintWriteRepository.ExistsForUserAndCommentAsync(request.UserId, request.CommentId, cancellationToken))
        {
            return Result.Failure(
                new Error("Complaint.AlreadySubmitted", "You have already filed a complaint against this comment.", ErrorType.Conflict));
        }

        var complaint = Complaint.Create(request.UserId, request.CommentId, request.Reason);
        complaintWriteRepository.Add(complaint);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
