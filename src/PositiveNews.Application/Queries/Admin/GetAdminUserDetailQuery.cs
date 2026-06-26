using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;

namespace PositiveNews.Application.Queries.Admin;

public sealed record GetAdminUserDetailQuery(long UserId) : IRequest<Result<UserAdminDetailDto>>;