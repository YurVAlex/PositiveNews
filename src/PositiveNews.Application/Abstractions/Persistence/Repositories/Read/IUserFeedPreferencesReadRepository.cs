using PositiveNews.Application.DTOs.FeedPreferences;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

/// <summary>
/// Read-only access to persisted user feed preferences and filters.
/// </summary>
public interface IUserFeedPreferencesReadRepository
{
    /// <summary>
    /// Loads preferences for the user, or null when no row exists yet.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Preference snapshot, or null when the user has no saved row.</returns>
    Task<UserFeedPreferencesDto?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}
