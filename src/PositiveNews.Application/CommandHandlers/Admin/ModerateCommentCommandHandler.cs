using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Commands.Admin;
using PositiveNews.Application.Common;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;

namespace PositiveNews.Application.CommandHandlers.Admin;

public sealed class ModerateCommentCommandHandler(
    ICommentWriteRepository commentWriteRepository,
    IAuditLogWriteRepository auditLogWriteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ModerateCommentCommand, Result>
{
    public async Task<Result> Handle(ModerateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await commentWriteRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(new Error(
                ErrorCodes.Admin.CommentNotFound,
                $"Comment with id '{request.CommentId}' was not found.",
                ErrorType.NotFound));
        }

        var changed = false;

        if (comment.IsActive != request.IsActive)
        {
            var oldValue = comment.IsActive.ToString();
            comment.SetActive(request.IsActive, request.ModeratorId);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.Comment,
                comment.Id,
                request.ModeratorId,
                nameof(Comment.IsActive),
                oldValue,
                comment.IsActive.ToString(),
                request.Reason,
                request.Note));
            changed = true;
        }

        if (!changed)
        {
            return Result.Failure(new Error(
                ErrorCodes.Admin.CommentUnchanged,
                "No changes were provided.",
                ErrorType.Validation));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
