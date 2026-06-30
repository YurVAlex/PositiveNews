using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Common;
using PositiveNews.Application.Commands.Admin;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;

namespace PositiveNews.Application.CommandHandlers.Admin;

public sealed class UpdateUserCommandHandler(
    IUserWriteRepository userWriteRepository,
    IAuditLogWriteRepository auditLogWriteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userWriteRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(new Error(
                ErrorCodes.Admin.UserNotFound,
                $"User with id '{request.UserId}' was not found.",
                ErrorType.NotFound));
        }

        var changed = false;

        if (user.IsActive != request.IsActive)
        {
            var oldValue = user.IsActive.ToString();
            user.SetActive(request.IsActive, request.ModeratorId);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.User,
                user.Id,
                request.ModeratorId,
                nameof(User.IsActive),
                oldValue,
                user.IsActive.ToString(),
                request.Reason,
                request.Note));
            changed = true;
        }

        if (user.EmailConfirmed != request.EmailConfirmed)
        {
            var oldValue = user.EmailConfirmed.ToString();
            user.SetEmailConfirmed(request.EmailConfirmed, request.ModeratorId);
            auditLogWriteRepository.Add(AuditLog.Create(
                AuditEntityType.User,
                user.Id,
                request.ModeratorId,
                nameof(User.EmailConfirmed),
                oldValue,
                user.EmailConfirmed.ToString(),
                request.Reason,
                request.Note));
            changed = true;
        }

        if (!changed)
        {
            return Result.Failure(new Error(
                ErrorCodes.Admin.UserUnchanged,
                "No changes were provided.",
                ErrorType.Validation));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}