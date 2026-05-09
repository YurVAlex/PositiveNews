using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;
using PositiveNews.Application.Queries.Auth;

namespace PositiveNews.Application.QueryHandlers.Auth;

public sealed class GetCurrentUserQueryHandler(IUserReadRepository userReadRepository)
    : IRequestHandler<GetCurrentUserQuery, Result<UserProfileModel>>
{
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
