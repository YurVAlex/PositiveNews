using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Auth;

/// <summary>
/// Deactivates the authenticated user's own account.
/// </summary>
/// <param name="UserId">Primary key of the user requesting deactivation.</param>
public sealed record DeactivateAccountCommand(long UserId) : IRequest<Result>;
