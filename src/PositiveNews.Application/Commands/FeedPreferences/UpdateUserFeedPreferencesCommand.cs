using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.FeedPreferences;

namespace PositiveNews.Application.Commands.FeedPreferences;

/// <summary>
/// Replaces the authenticated user's feed preference snapshot.
/// </summary>
/// <param name="UserId">User identifier from the security context.</param>
/// <param name="TopicNames">Preferred topic names.</param>
/// <param name="SourceIds">Preferred source ids.</param>
/// <param name="MinPositivity">Minimum positivity threshold in [0, 1].</param>
/// <param name="SortBy">Sort mode: <c>date</c>, <c>positivity</c>, or <c>preferences</c>.</param>
public sealed record UpdateUserFeedPreferencesCommand(
    long UserId,
    IReadOnlyList<string> TopicNames,
    IReadOnlyList<int> SourceIds,
    decimal MinPositivity,
    string SortBy) : IRequest<Result<UserFeedPreferencesDto>>;
