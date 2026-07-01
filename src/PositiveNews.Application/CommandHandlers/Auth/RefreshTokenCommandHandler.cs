using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Auth;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.CommandHandlers.Auth;

/// <summary>
/// Validates a refresh token and issues a new access token.
/// </summary>
/// <param name="refreshTokenReadRepository">Loads refresh tokens.</param>
/// <param name="refreshTokenWriteRepository">Persists refresh token changes.</param>
/// <param name="tokenService">Creates JWT access tokens and refresh tokens.</param>
/// <param name="unitOfWork">Commits the transaction.</param>
public sealed class RefreshTokenCommandHandler(
    IRefreshTokenReadRepository refreshTokenReadRepository,
    IRefreshTokenWriteRepository refreshTokenWriteRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, Result<AuthResultModel>>
{
    /// <summary>
    /// Validates the refresh token, revokes it, and returns a new access token and refresh token.
    /// </summary>
    /// <param name="request">The refresh token command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication bundle or a typed application error.</returns>
    public async Task<Result<AuthResultModel>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingRefreshToken = await refreshTokenReadRepository.FindValidByTokenAsync(request.RefreshToken, cancellationToken);
        if (existingRefreshToken is null)
        {
            return Result<AuthResultModel>.Failure(
                new Error(ErrorCodes.Auth.InvalidRefreshToken, "Invalid or expired refresh token.", ErrorType.Unauthorized));
        }

        var user = existingRefreshToken.User;
        if (!user.IsActive)
        {
            return Result<AuthResultModel>.Failure(
                new Error(ErrorCodes.Auth.UserInactive, "The user has been deleted or blocked.", ErrorType.Unauthorized));
        }

        // Revoke the old refresh token
        existingRefreshToken.Revoke();
        refreshTokenWriteRepository.Update(existingRefreshToken);

        // Create a new refresh token
        var newRefreshTokenString = tokenService.CreateRefreshTokenString();
        var newRefreshToken = RefreshToken.Create(newRefreshTokenString, user.Id, tokenService.GetRefreshTokenExpiryUtc());
        refreshTokenWriteRepository.Add(newRefreshToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        return Result<AuthResultModel>.Success(new AuthResultModel
        {
            AccessToken = tokenService.CreateAccessToken(user, roles),
            ExpiresAtUtc = tokenService.GetAccessTokenExpiryUtc(),
            RefreshToken = newRefreshTokenString,
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
