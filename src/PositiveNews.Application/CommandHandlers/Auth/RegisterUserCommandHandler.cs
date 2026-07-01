using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Auth;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.CommandHandlers.Auth;

/// <summary>
/// Registers a new account with the default User role and returns an authenticated session.
/// </summary>
/// <param name="userReadRepository">Checks email uniqueness.</param>
/// <param name="userWriteRepository">Persists new users.</param>
/// <param name="userRoleWriteRepository">Assigns roles to users.</param>
/// <param name="roleReadRepository">Loads the default role entity.</param>
/// <param name="passwordHasherService">Hashes passwords for storage.</param>
/// <param name="tokenService">Issues JWT access tokens and refresh tokens.</param>
/// <param name="refreshTokenWriteRepository">Persists refresh tokens.</param>
/// <param name="unitOfWork">Commits the transactional registration.</param>
public sealed class RegisterUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserWriteRepository userWriteRepository,
    IUserRoleWriteRepository userRoleWriteRepository,
    IUserFeedPreferencesWriteRepository userFeedPreferencesWriteRepository,
    IRoleReadRepository roleReadRepository,
    IPasswordHasherService passwordHasherService,
    ITokenService tokenService,
    IRefreshTokenWriteRepository refreshTokenWriteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterUserCommand, Result<AuthResultModel>>
{
    /// <summary>
    /// Creates the user, assigns the default role, hashes the password, and returns tokens plus profile on success.
    /// </summary>
    /// <param name="request">Registration payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication bundle or a typed application error.</returns>
    public async Task<Result<AuthResultModel>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedName = request.Name.Trim();

        if (await userReadRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return Result<AuthResultModel>.Failure(
                new Error(ErrorCodes.Auth.EmailAlreadyExists, "A user with this email already exists.", ErrorType.Conflict));
        }

        var userRole = await roleReadRepository.FindByNameAsync(RoleNames.User, cancellationToken);
        if (userRole is null)
        {
            return Result<AuthResultModel>.Failure(
                new Error(ErrorCodes.Auth.RoleMissing, "Default 'User' role is missing.", ErrorType.Unexpected));
        }

        var user = User.Create(normalizedEmail, normalizedName);
        user.ConfirmEmail();
        user.SetPasswordHash(passwordHasherService.HashPassword(user, request.Password));
        user.RecordSuccessfulLogin();

        userWriteRepository.Add(user);
        userRoleWriteRepository.Add(UserRole.Create(userRole.Id, user));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        userFeedPreferencesWriteRepository.AddDefault(user.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = new[] { userRole.Name };
        var refreshTokenString = tokenService.CreateRefreshTokenString();
        var refreshToken = RefreshToken.Create(refreshTokenString, user.Id, tokenService.GetRefreshTokenExpiryUtc());
        refreshTokenWriteRepository.Add(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResultModel>.Success(new AuthResultModel
        {
            AccessToken = tokenService.CreateAccessToken(user, roles),
            ExpiresAtUtc = tokenService.GetAccessTokenExpiryUtc(),
            RefreshToken = refreshTokenString,
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
