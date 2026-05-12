using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Application.Commands.Auth;

/// <summary>
/// Creates a new user account with the default role and returns an authenticated session.
/// </summary>
/// <param name="Email">Unique email address for the account.</param>
/// <param name="Name">Public display name.</param>
/// <param name="Password">Plain-text password meeting complexity rules.</param>
public sealed record RegisterUserCommand(string Email, string Name, string Password) : IRequest<Result<AuthResultModel>>;
