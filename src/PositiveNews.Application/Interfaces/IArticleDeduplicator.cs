using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IArticleDeduplicator
{
    bool MatchesExisting(ExistingArticleKeys keys, RssFeedItemDto dto);
    bool ConflictsWithPending(
        RssFeedItemDto item,
        HashSet<string> pendingExternalIds,
        HashSet<string> pendingUrls,
        HashSet<string> pendingTitles);
}
