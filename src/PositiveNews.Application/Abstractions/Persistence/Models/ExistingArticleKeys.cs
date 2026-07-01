namespace PositiveNews.Application.Abstractions.Persistence.Models;

/// <summary>
/// Keys already present in storage, used to skip duplicates without per-item queries.
/// </summary>
/// <param name="ExternalIds">Set of stored external identifiers.</param>
/// <param name="Urls">Set of stored canonical URLs.</param>
/// <param name="Titles">Set of stored titles used for fuzzy dedupe.</param>
public sealed record ExistingArticleKeys(
    IReadOnlySet<string> ExternalIds,
    IReadOnlySet<string> Urls,
    IReadOnlySet<string> Titles);
