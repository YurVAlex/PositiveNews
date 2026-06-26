using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class UserFeedPreferencesWriteRepository(AppDbContext db) : IUserFeedPreferencesWriteRepository
{
    /// <inheritdoc />
    public void AddDefault(long userId)
    {
        if (db.UserFeedPreferences.Local.Any(p => p.UserId == userId)
            || db.UserFeedPreferences.Any(p => p.UserId == userId))
        {
            return;
        }

        db.UserFeedPreferences.Add(UserFeedPreference.Create(userId));
    }

    /// <inheritdoc />
    public async Task ReplacePreferencesAsync(
        long userId,
        decimal minPositivity,
        string storedSortBy,
        IReadOnlyList<int> topicIds,
        IReadOnlyList<int> sourceIds,
        CancellationToken cancellationToken = default)
    {
        var preference = await db.UserFeedPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (preference is null)
        {
            preference = UserFeedPreference.Create(userId, minPositivity, storedSortBy);
            db.UserFeedPreferences.Add(preference);
        }
        else
        {
            preference.UpdatePreferences(minPositivity, storedSortBy, null, null);
        }

        var existingTopics = await db.UserTopicFilters
            .Where(f => f.UserId == userId)
            .ToListAsync(cancellationToken);
        if (existingTopics.Count > 0)
        {
            db.UserTopicFilters.RemoveRange(existingTopics);
        }

        foreach (var topicId in topicIds.Distinct())
        {
            db.UserTopicFilters.Add(UserTopicFilter.Create(userId, topicId));
        }

        var existingSources = await db.UserSourceFilters
            .Where(f => f.UserId == userId)
            .ToListAsync(cancellationToken);
        if (existingSources.Count > 0)
        {
            db.UserSourceFilters.RemoveRange(existingSources);
        }

        foreach (var sourceId in sourceIds.Distinct())
        {
            db.UserSourceFilters.Add(UserSourceFilter.Create(userId, sourceId));
        }
    }
}
