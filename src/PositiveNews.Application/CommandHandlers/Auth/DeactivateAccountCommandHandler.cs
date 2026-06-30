using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Application.CommandHandlers.Auth;

/// <summary>
/// Soft-deactivates the current user's account and records self-moderation.
/// </summary>
public sealed class DeactivateAccountCommandHandler(
    IUserReadRepository userReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateAccountCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(
                new Error(ErrorCodes.Auth.UserNotFound, "User account was not found.", ErrorType.NotFound));
        }

        if (!user.IsActive)
        {
            return Result.Failure(
                new Error(ErrorCodes.Auth.AccountInactive, "Account is already deactivated.", ErrorType.Conflict));
        }

        try
        {
            user.Deactivate(user.Id);
        }
        catch (InvalidUserStateException ex)
        {
            return Result.Failure(
                new Error(ErrorCodes.Auth.AccountInactive, ex.Message, ErrorType.Conflict));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
