namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Persists user feed preference rows and related filter tables.
/// </summary>
public interface IUserFeedPreferencesWriteRepository
{
    /// <summary>
    /// Stages default feed preference row for a newly registered user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    void AddDefault(long userId);

    /// <summary>
    /// Upserts core preference fields and replaces topic/source filter rows for the user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="minPositivity">Minimum positivity threshold.</param>
    /// <param name="storedSortBy">Persisted sort mode string.</param>
    /// <param name="topicIds">Resolved topic ids for filters.</param>
    /// <param name="sourceIds">Source ids for filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReplacePreferencesAsync(
        long userId,
        decimal minPositivity,
        string storedSortBy,
        IReadOnlyList<int> topicIds,
        IReadOnlyList<int> sourceIds,
        CancellationToken cancellationToken = default);
}
