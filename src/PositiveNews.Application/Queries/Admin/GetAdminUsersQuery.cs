using MediatR;
using PositiveNews.Application.DTOs.Admin;

namespace PositiveNews.Application.Queries.Admin;

public sealed record GetAdminUsersQuery(string? SearchTerm) : IRequest<IReadOnlyList<UserAdminItemDto>>;