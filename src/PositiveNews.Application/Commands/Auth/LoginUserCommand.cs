using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Application.Commands.Auth;

/// <summary>
/// Authenticates a user by email and password and returns tokens plus profile.
/// </summary>
/// <param name="Email">Account email address.</param>
/// <param name="Password">Plain-text password.</param>
public sealed record LoginUserCommand(string Email, string Password) : IRequest<Result<AuthResultModel>>;
