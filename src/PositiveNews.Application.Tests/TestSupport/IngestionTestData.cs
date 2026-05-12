using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Tests.TestSupport;

internal static class IngestionTestData
{
    public static IngestionSettingsSnapshot MinimalSettings()
        => new(
            new PositivityAnalizerKeyPhrases(
                PositiveWords: new HashSet<string>(),
                NegativeWords: new HashSet<string>(),
                PositivePhrases: new HashSet<string>(),
                NegativePhrases: new HashSet<string>(),
                NegationWords: new HashSet<string>(),
                IntensifierWords: new HashSet<string>(),
                NegationLookbackTokens: 1,
                IntensifierLookbackTokens: 1,
                IntensifierMultiplier: 1m,
                PhrasePolarityWeight: 1m),
            new CleanerRules(
                StopProcessingPatterns: [],
                RemoveNodePatterns: [],
                RemoveDivClassPatterns: [],
                ShouldRemoveParagraphPatterns: [],
                AllowedTags: new HashSet<string>(),
                AttributesToRemove: new HashSet<string>()),
            new FeedItemValidationRules(InvalidAuthors: new HashSet<string>()),
            Sources: new Dictionary<string, SourceIngestionRules>(StringComparer.OrdinalIgnoreCase));

    public static TopicLookup EmptyTopicLookup()
        => new(
            ByName: new Dictionary<string, TopicSnapshot>(StringComparer.OrdinalIgnoreCase),
            BySlugWord: new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase),
            ChildToParentTopics: new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase));

    public static IngestionSourceSnapshot ValidSource(int id = 1)
        => new(id, "Test Source", "https://example.com/feed.xml", "en", null);
}
