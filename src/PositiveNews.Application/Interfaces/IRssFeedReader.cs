using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

/// <summary>
/// Abstracts RSS feed fetching so the service can be tested without HTTP calls.
/// </summary>
public interface IRssFeedReader
{
    /// <summary>
    /// Fetches and parses an RSS/Atom feed from the given URL.
    /// </summary>
    Task<IReadOnlyList<RssFeedItemDto>> ReadFeedAsync(string feedUrl, CancellationToken cancellationToken = default);
}