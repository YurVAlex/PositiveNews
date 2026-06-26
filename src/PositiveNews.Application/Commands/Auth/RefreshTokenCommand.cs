using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Application.Commands.Auth;

/// <summary>
/// Refreshes an access token using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token string.</param>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResultModel>>;
