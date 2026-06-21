using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;

namespace PositiveNews.Application.QueryHandlers.Admin;

public sealed class GetAdminUsersQueryHandler(IUserReadRepository userReadRepository)
    : IRequestHandler<GetAdminUsersQuery, IReadOnlyList<UserAdminItemDto>>
{
    public Task<IReadOnlyList<UserAdminItemDto>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
        => userReadRepository.SearchAdminUsersAsync(request.SearchTerm, cancellationToken);
}