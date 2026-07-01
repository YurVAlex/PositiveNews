using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Commands.FeedPreferences;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.FeedPreferences;
using PositiveNews.Application.Mapping;

namespace PositiveNews.Application.CommandHandlers.FeedPreferences;

/// <summary>
/// Validates and persists a full feed preference snapshot for the user.
/// </summary>
public sealed class UpdateUserFeedPreferencesCommandHandler(
    ITopicReadRepository topicReadRepository,
    ISourceReadRepository sourceReadRepository,
    IUserFeedPreferencesWriteRepository preferencesWriteRepository,
    IUserFeedPreferencesReadRepository preferencesReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserFeedPreferencesCommand, Result<UserFeedPreferencesDto>>
{
    /// <inheritdoc />
    public async Task<Result<UserFeedPreferencesDto>> Handle(
        UpdateUserFeedPreferencesCommand request,
        CancellationToken cancellationToken)
    {
        var topics = NormalizeTopics(request.TopicNames);
        if (topics.Count > 0)
        {
            var knownTopics = await topicReadRepository.GetTopicNamesAsync(cancellationToken);
            var knownTopicSet = new HashSet<string>(knownTopics, StringComparer.OrdinalIgnoreCase);
            var missingTopics = topics.Where(topic => !knownTopicSet.Contains(topic)).ToArray();
            if (missingTopics.Length > 0)
            {
                return Result<UserFeedPreferencesDto>.Failure(
                    new Error(
                        ErrorCodes.FeedPreferences.TopicNotFound,
                        $"Requested topic(s) were not found: {string.Join(", ", missingTopics)}.",
                        ErrorType.NotFound));
            }
        }

        var sourceIds = NormalizeSourceIds(request.SourceIds);
        if (sourceIds.Count > 0)
        {
            var existingSourceIds = await sourceReadRepository.GetExistingSourceIdsAsync(sourceIds, cancellationToken);
            var existingSourceIdSet = existingSourceIds.ToHashSet();
            var missingSourceIds = sourceIds.Where(id => !existingSourceIdSet.Contains(id)).ToArray();
            if (missingSourceIds.Length > 0)
            {
                return Result<UserFeedPreferencesDto>.Failure(
                    new Error(
                        ErrorCodes.FeedPreferences.SourceNotFound,
                        $"Requested source(s) were not found: {string.Join(", ", missingSourceIds)}.",
                        ErrorType.NotFound));
            }
        }

        var topicIdMap = topics.Count > 0
            ? await topicReadRepository.GetTopicIdsByNamesAsync(topics, cancellationToken)
            : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var topicIds = topics
            .Select(name => topicIdMap.TryGetValue(name, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        var storedSort = FeedPreferenceSortMapper.ToStoredSort(request.SortBy);

        await preferencesWriteRepository.ReplacePreferencesAsync(
            request.UserId,
            request.MinPositivity,
            storedSort,
            topicIds,
            sourceIds,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await preferencesReadRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (saved is null)
        {
            return Result<UserFeedPreferencesDto>.Failure(
                new Error(ErrorCodes.FeedPreferences.SaveFailed, "Preferences could not be loaded after save.", ErrorType.Unexpected));
        }

        return Result<UserFeedPreferencesDto>.Success(saved);
    }

    private static IReadOnlyList<string> NormalizeTopics(IReadOnlyList<string>? topics)
    {
        return (topics ?? Array.Empty<string>())
            .Where(topic => !string.IsNullOrWhiteSpace(topic))
            .Select(topic => topic.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<int> NormalizeSourceIds(IReadOnlyList<int>? sourceIds)
    {
        return (sourceIds ?? Array.Empty<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToArray();
    }
}
