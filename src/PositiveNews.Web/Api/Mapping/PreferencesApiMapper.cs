using PositiveNews.Application.Commands.FeedPreferences;
using PositiveNews.Application.DTOs.FeedPreferences;
using PositiveNews.Web.Api.Models;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Web.Api.Mapping;

/// <summary>
/// Mapperly mappings for user feed preference API models.
/// </summary>
[Mapper]
public static partial class PreferencesApiMapper
{
    /// <summary>
    /// Maps application preferences to the wire response.
    /// </summary>
    public static partial UserFeedPreferencesResponse ToUserFeedPreferencesResponse(this UserFeedPreferencesDto source);

    /// <summary>
    /// Maps an update request to the application command.
    /// </summary>
    public static UpdateUserFeedPreferencesCommand ToUpdateUserFeedPreferencesCommand(
        this UpdateUserFeedPreferencesRequest source,
        long userId)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new UpdateUserFeedPreferencesCommand(
            userId,
            source.TopicNames ?? Array.Empty<string>(),
            source.SourceIds ?? Array.Empty<int>(),
            source.MinPositivity,
            source.SortBy ?? "date");
    }
}
