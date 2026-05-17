using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;
using PositiveNews.Application.Queries.Auth;

namespace PositiveNews.Application.QueryHandlers.Auth;

/// <summary>
/// Loads the active user's profile and roles by identifier for session/bootstrap endpoints.
/// </summary>
/// <param name="userReadRepository">Reads users with role joins.</param>
public sealed class GetCurrentUserQueryHandler(IUserReadRepository userReadRepository)
    : IRequestHandler<GetCurrentUserQuery, Result<UserProfileModel>>
{
    /// <summary>
    /// Returns profile data when the user exists and is active; otherwise returns unauthorized error.
    /// </summary>
    /// <param name="request">Contains the user id from the security context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Profile model or typed failure.</returns>
    public async Task<Result<UserProfileModel>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<UserProfileModel>.Failure(
                new Error("Auth.UserUnavailable", "Current user is unavailable.", ErrorType.Unauthorized));
        }

        return Result<UserProfileModel>.Success(new UserProfileModel
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray()
        });
    }
}
