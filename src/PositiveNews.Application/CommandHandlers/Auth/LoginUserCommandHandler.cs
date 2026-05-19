using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Application.CommandHandlers.Auth;

/// <summary>
/// Authenticates credentials, updates login audit fields, and issues an access token with roles.
/// </summary>
/// <param name="userReadRepository">Loads users by email for credential checks.</param>
/// <param name="passwordHasherService">Verifies passwords against stored hashes.</param>
/// <param name="tokenService">Creates JWT access tokens.</param>
/// <param name="unitOfWork">Commits login attempt side effects.</param>
public sealed class LoginUserCommandHandler(
    IUserReadRepository userReadRepository,
    IPasswordHasherService passwordHasherService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginUserCommand, Result<AuthResultModel>>
{
    /// <summary>
    /// Validates the user is active, verifies the password, persists login telemetry, and returns tokens plus profile.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication bundle or a typed application error.</returns>
    public async Task<Result<AuthResultModel>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await userReadRepository.FindByEmailWithRolesAsync(normalizedEmail, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return Result<AuthResultModel>.Failure(
                new Error("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.Unauthorized));
        }
        if (!user.IsActive)
        {
            return Result<AuthResultModel>.Failure(
                new Error("Auth.InvalidCredentials", "The user has been deleted or blocked.", ErrorType.Unauthorized));
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
