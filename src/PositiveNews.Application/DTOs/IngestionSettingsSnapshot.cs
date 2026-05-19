namespace PositiveNews.Application.DTOs;

/// <summary>
/// Immutable bundle of configuration used by the RSS ingestion pipeline for one cycle.
/// </summary>
/// <param name="PositivityAnalizerKeyPhrases">Lexicons and weights for sentiment scoring.</param>
/// <param name="CleanerRules">HTML stripping and allow-list settings.</param>
/// <param name="FeedItemValidationRules">Validation thresholds for rejecting feed items.</param>
/// <param name="Sources">Per-source overrides keyed by URL fragment or name.</param>
public sealed record IngestionSettingsSnapshot(
    PositivityAnalizerKeyPhrases PositivityAnalizerKeyPhrases,
    CleanerRules CleanerRules,
    FeedItemValidationRules FeedItemValidationRules,
    IReadOnlyDictionary<string, SourceIngestionRules> Sources);

/// <summary>
/// Tunable positivity analyzer dictionaries and numeric knobs.
/// </summary>
/// <param name="PositiveWords">Single tokens counted as positive sentiment.</param>
/// <param name="NegativeWords">Single tokens counted as negative sentiment.</param>
/// <param name="PositivePhrases">Multi-word positive phrases.</param>
/// <param name="NegativePhrases">Multi-word negative phrases.</param>
/// <param name="NegationWords">Words that flip polarity of following tokens.</param>
/// <param name="IntensifierWords">Words that amplify the next polar token.</param>
/// <param name="NegationLookbackTokens">How far negation may reach backward.</param>
/// <param name="IntensifierLookbackTokens">How far intensifiers may reach backward.</param>
/// <param name="IntensifierMultiplier">Scale applied when an intensifier is active.</param>
/// <param name="PhrasePolarityWeight">Relative weight of phrase hits versus token hits.</param>
public sealed record PositivityAnalizerKeyPhrases(
    IReadOnlySet<string> PositiveWords,
    IReadOnlySet<string> NegativeWords,
    IReadOnlySet<string> PositivePhrases,
    IReadOnlySet<string> NegativePhrases,
    IReadOnlySet<string> NegationWords,
    IReadOnlySet<string> IntensifierWords,
    int NegationLookbackTokens,
    int IntensifierLookbackTokens,
    decimal IntensifierMultiplier,
    decimal PhrasePolarityWeight);

/// <summary>
/// Patterns controlling HTML cleanup during ingestion.
/// </summary>
/// <param name="StopProcessingPatterns">When matched, the entire item may be skipped.</param>
/// <param name="RemoveNodePatterns">XPath-like or substring patterns for node removal.</param>
/// <param name="RemoveDivClassPatterns">CSS class names whose divs should be stripped.</param>
/// <param name="ShouldRemoveParagraphPatterns">Paragraph-level removal heuristics.</param>
/// <param name="AllowedTags">HTML tags permitted after sanitization.</param>
/// <param name="AttributesToRemove">Attribute names stripped from surviving tags.</param>
public sealed record CleanerRules(
    IReadOnlyList<string> StopProcessingPatterns,
    IReadOnlyList<string> RemoveNodePatterns,
    IReadOnlyList<string> RemoveDivClassPatterns,
    IReadOnlyList<string> ShouldRemoveParagraphPatterns,
    IReadOnlySet<string> AllowedTags,
    IReadOnlySet<string> AttributesToRemove);

/// <summary>
/// Rules used by <see cref="PositiveNews.Application.Interfaces.IFeedItemValidator"/> before deeper processing.
/// </summary>
/// <param name="InvalidAuthors">Author names or patterns that disqualify an item.</param>
/// <param name="InvalidLinkContains">Link substrings that disqualify an item when present.</param>
public sealed record FeedItemValidationRules(
    IReadOnlySet<string> InvalidAuthors,
    IReadOnlyList<string> InvalidLinkContains);

/// <summary>
/// Optional overrides when a feed URL matches <see cref="UrlContains"/>.
/// </summary>
/// <param name="UrlContains">Substring that must appear in the feed URL for rules to apply.</param>
/// <param name="DefaultTopics">Topics automatically applied to matching feeds.</param>
public sealed record SourceIngestionRules(
    string UrlContains,
    IReadOnlyList<string> DefaultTopics);
