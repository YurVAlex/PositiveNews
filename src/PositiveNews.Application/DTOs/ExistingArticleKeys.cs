namespace PositiveNews.Application.DTOs;

/// <summary>
/// Keys already present in storage, used to skip duplicates without per-item queries.
/// </summary>
public sealed class ExistingArticleKeys
{
    public HashSet<string> ExternalIds { get; } = new(StringComparer.Ordinal);
    public HashSet<string> Urls { get; } = new(StringComparer.Ordinal);
    public HashSet<string> Titles { get; } = new(StringComparer.Ordinal);

    public bool Matches(RssFeedItemDto dto) =>
        (!string.IsNullOrEmpty(dto.ExternalId) && ExternalIds.Contains(dto.ExternalId))
        || Urls.Contains(dto.Link)
        || Titles.Contains(dto.Title);
}
