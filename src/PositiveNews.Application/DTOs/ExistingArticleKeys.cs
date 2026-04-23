namespace PositiveNews.Application.DTOs;

/// <summary>
/// Keys already present in storage, used to skip duplicates without per-item queries.
/// </summary>
public sealed record ExistingArticleKeys(
    IReadOnlySet<string> ExternalIds,
    IReadOnlySet<string> Urls,
    IReadOnlySet<string> Titles);
