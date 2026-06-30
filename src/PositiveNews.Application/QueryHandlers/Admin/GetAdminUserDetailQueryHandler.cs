using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.Queries.Admin;

namespace PositiveNews.Application.QueryHandlers.Admin;

public sealed class GetAdminUserDetailQueryHandler(IUserReadRepository userReadRepository)
    : IRequestHandler<GetAdminUserDetailQuery, Result<PositiveNews.Application.DTOs.Admin.UserAdminDetailDto>>
{
    public async Task<Result<PositiveNews.Application.DTOs.Admin.UserAdminDetailDto>> Handle(
        GetAdminUserDetailQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.GetAdminUserDetailAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<PositiveNews.Application.DTOs.Admin.UserAdminDetailDto>.Failure(new Error(
                ErrorCodes.Admin.UserNotFound,
                $"User with id '{request.UserId}' was not found.",
                ErrorType.NotFound));
        }

        return Result<PositiveNews.Application.DTOs.Admin.UserAdminDetailDto>.Success(user);
    }
}