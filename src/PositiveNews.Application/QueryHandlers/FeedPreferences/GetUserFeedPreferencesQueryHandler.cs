using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.FeedPreferences;
using PositiveNews.Application.Mapping;
using PositiveNews.Application.Queries.FeedPreferences;
using PositiveNews.Domain.Constants;

namespace PositiveNews.Application.QueryHandlers.FeedPreferences;

/// <summary>
/// Returns saved feed preferences or application defaults when none exist.
/// </summary>
public sealed class GetUserFeedPreferencesQueryHandler(IUserFeedPreferencesReadRepository preferencesReadRepository)
    : IRequestHandler<GetUserFeedPreferencesQuery, Result<UserFeedPreferencesDto>>
{
    /// <inheritdoc />
    public async Task<Result<UserFeedPreferencesDto>> Handle(
        GetUserFeedPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        var stored = await preferencesReadRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (stored is not null)
        {
            return Result<UserFeedPreferencesDto>.Success(stored);
        }

        return Result<UserFeedPreferencesDto>.Success(new UserFeedPreferencesDto(
            Array.Empty<string>(),
            Array.Empty<int>(),
            FeedPreferenceDefaults.MinPositivity,
            FeedPreferenceSortMapper.DefaultApiSort));
    }
}
