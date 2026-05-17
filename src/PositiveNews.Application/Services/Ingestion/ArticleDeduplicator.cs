using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Application.Services.Ingestion;

/// <summary>
/// Default duplicate detection comparing external ids, canonical URLs, and titles against DB and pending buffers.
/// </summary>
internal sealed class ArticleDeduplicator : IArticleDeduplicator
{
    /// <summary>
    /// Returns true when any persisted key matches the item's external id, URL, or title.
    /// </summary>
    public bool MatchesExisting(ExistingArticleKeys keys, RssFeedItemDto dto)
    {
        return (!string.IsNullOrEmpty(dto.ExternalId) && keys.ExternalIds.Contains(dto.ExternalId))
            || keys.Urls.Contains(dto.Link)
            || keys.Titles.Contains(dto.Title);
    }

    /// <summary>
    /// Returns true when the item repeats an external id, URL, or title already staged in the current batch.
    /// </summary>
    public bool ConflictsWithPending(
        RssFeedItemDto item,
        HashSet<string> pendingExternalIds,
        HashSet<string> pendingUrls,
        HashSet<string> pendingTitles)
    {
        if (!string.IsNullOrEmpty(item.ExternalId) && pendingExternalIds.Contains(item.ExternalId))
            return true;
        if (pendingUrls.Contains(item.Link))
            return true;
        return pendingTitles.Contains(item.Title);
    }
}
