using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Admin;

public sealed record UpdateUserCommand(
    long UserId,
    bool IsActive,
    bool EmailConfirmed,
    string? Reason,
    string? Note,
    long ModeratorId) : IRequest<Result>;