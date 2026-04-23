using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Services.Ingestion;

internal sealed class ArticleDeduplicator : IArticleDeduplicator
{
    public bool MatchesExisting(ExistingArticleKeys keys, RssFeedItemDto dto)
    {
        return (!string.IsNullOrEmpty(dto.ExternalId) && keys.ExternalIds.Contains(dto.ExternalId))
            || keys.Urls.Contains(dto.Link)
            || keys.Titles.Contains(dto.Title);
    }

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
