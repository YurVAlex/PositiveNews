namespace PositiveNews.Application.DTOs;

public sealed record IngestionSettingsSnapshot(
    PositivityAnalizerKeyPhrases PositivityAnalizerKeyPhrases,
    CleanerRules CleanerRules,
    FeedItemValidationRules FeedItemValidationRules,
    IReadOnlyDictionary<string, SourceIngestionRules> Sources);

public sealed record PositivityAnalizerKeyPhrases(
    IReadOnlySet<string> PositiveWords,
    IReadOnlySet<string> NegativeWords);

public sealed record CleanerRules(
    IReadOnlyList<string> StopProcessingPatterns,
    IReadOnlyList<string> RemoveNodePatterns,
    IReadOnlyList<string> RemoveDivClassPatterns,
    IReadOnlyList<string> ShouldRemoveParagraphPatterns,
    IReadOnlySet<string> AllowedTags,
    IReadOnlySet<string> AttributesToRemove);

public sealed record FeedItemValidationRules(
    IReadOnlySet<string> InvalidAuthors);

public sealed record SourceIngestionRules(
    string UrlContains,
    IReadOnlyList<string> DefaultTopics);
