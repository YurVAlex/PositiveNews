using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Application.Queries.Auth;

/// <summary>
/// Resolves the profile for the authenticated user id from the security context.
/// </summary>
/// <param name="UserId">Authenticated user's primary key.</param>
public sealed record GetCurrentUserQuery(long UserId) : IRequest<Result<UserProfileModel>>;
