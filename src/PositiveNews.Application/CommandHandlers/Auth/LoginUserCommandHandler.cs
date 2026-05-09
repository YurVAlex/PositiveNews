using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Application.CommandHandlers.Auth;

public sealed class LoginUserCommandHandler(
    IUserReadRepository userReadRepository,
    IPasswordHasherService passwordHasherService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginUserCommand, Result<AuthResultModel>>
{
    public async Task<Result<AuthResultModel>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await userReadRepository.FindByEmailWithRolesAsync(normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return Result<AuthResultModel>.Failure(
                new Error("Auth.InvalidCredentials", "Invalid email or password. Is the user blocked?", ErrorType.Unauthorized));
        }

        if (!passwordHasherService.VerifyPassword(user, user.PasswordHash, request.Password))
        {
            user.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<AuthResultModel>.Failure(
                new Error("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.Unauthorized));
        }

        user.RecordSuccessfulLogin();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        return Result<AuthResultModel>.Success(new AuthResultModel
        {
            AccessToken = tokenService.CreateAccessToken(user, roles),
            ExpiresAtUtc = tokenService.GetAccessTokenExpiryUtc(),
            User = new UserProfileModel
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Roles = roles
            }
        });
    }
}
