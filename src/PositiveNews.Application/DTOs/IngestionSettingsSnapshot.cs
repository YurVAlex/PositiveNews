namespace PositiveNews.Application.DTOs;

public sealed record IngestionSettingsSnapshot(
    CommonIngestionRules Common,
    IReadOnlyDictionary<string, SourceIngestionRules> Sources);

public sealed record CommonIngestionRules(
    IReadOnlySet<string> PositiveWords,
    IReadOnlySet<string> NegativeWords,
    IReadOnlyList<string> StopProcessingPatterns,
    IReadOnlyList<string> RemoveNodePatterns,
    IReadOnlyList<string> RemoveDivClassPatterns,
    IReadOnlyList<string> ShouldRemoveParagraphPatterns,
    IReadOnlySet<string> AllowedTags,
    IReadOnlySet<string> AttributesToRemove);

public sealed record SourceIngestionRules(
    string UrlContains,
    IReadOnlyList<string> DefaultTopics);
