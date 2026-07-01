using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Tests.TestSupport;

internal static class IngestionTestData
{
    public static PositivityAnalizerKeyPhrases EmptyPositivityLexicon()
        => new(
            PositiveWords: new HashSet<string>(),
            NegativeWords: new HashSet<string>(),
            PositivePhrases: new HashSet<string>(),
            NegativePhrases: new HashSet<string>(),
            NegationWords: new HashSet<string>(),
            IntensifierWords: new HashSet<string>(),
            NegationLookbackTokens: 1,
            IntensifierLookbackTokens: 1,
            IntensifierMultiplier: 1m,
            PhrasePolarityWeight: 1m,
            MitigationWords: new HashSet<string>(),
            MitigationPhrases: new HashSet<string>(),
            MitigationLookbackTokens: 1,
            TitleWeight: 0.15m,
            LedeWeight: 0.35m,
            BodyWeight: 0.50m,
            LedeCharCount: 500);

    public static IngestionSettingsSnapshot MinimalSettings()
        => new(
            EmptyPositivityLexicon(),
            new CleanerRules(
                StopProcessingPatterns: [],
                RemoveNodePatterns: [],
                RemoveDivClassPatterns: [],
                ShouldRemoveParagraphPatterns: [],
                AllowedTags: new HashSet<string>(),
                AttributesToRemove: new HashSet<string>()),
            new FeedItemValidationRules(InvalidAuthors: new HashSet<string>(), InvalidLinkContains: []),
            Sources: new Dictionary<string, SourceIngestionRules>(StringComparer.OrdinalIgnoreCase));

    public static TopicLookup EmptyTopicLookup()
        => new(
            ByName: new Dictionary<string, TopicSnapshot>(StringComparer.OrdinalIgnoreCase),
            BySlugWord: new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase),
            ChildToParentTopics: new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase));

    public static TopicLookup TopicLookupWith(params (string Name, int Id)[] topics)
    {
        var byName = topics.ToDictionary(
            t => t.Name,
            t => new TopicSnapshot(t.Id, t.Name, string.Empty, null),
            StringComparer.OrdinalIgnoreCase);

        return new TopicLookup(
            byName,
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase));
    }

    public static IngestionSourceSnapshot ValidSource(int id = 1)
        => new(id, "Test Source", "https://example.com/feed.xml", "en", null);
}
