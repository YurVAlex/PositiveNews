using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.FeedPreferences;
using PositiveNews.Application.Features.FeedPreferences;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class UserFeedPreferencesReadRepository(AppDbContext db) : IUserFeedPreferencesReadRepository
{
    /// <inheritdoc />
    public async Task<UserFeedPreferencesDto?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var preference = await db.UserFeedPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (preference is null)
        {
            return null;
        }

        var topicNames = await db.UserTopicFilters
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Join(
                db.Topics.AsNoTracking(),
                f => f.TopicId,
                t => t.Id,
                (_, t) => t.Name)
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);

        var sourceIds = await db.UserSourceFilters
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => f.SourceId)
            .OrderBy(id => id)
            .ToListAsync(cancellationToken);

        return new UserFeedPreferencesDto(
            topicNames,
            sourceIds,
            preference.MinPositivity,
            FeedPreferenceSortMapper.ToApiSort(preference.SortBy));
    }
}
