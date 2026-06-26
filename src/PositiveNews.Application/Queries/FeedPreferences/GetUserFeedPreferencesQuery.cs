using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.FeedPreferences;

namespace PositiveNews.Application.Queries.FeedPreferences;

/// <summary>
/// Loads saved feed preferences for the authenticated user.
/// </summary>
/// <param name="UserId">User identifier from the security context.</param>
public sealed record GetUserFeedPreferencesQuery(long UserId) : IRequest<Result<UserFeedPreferencesDto>>;
