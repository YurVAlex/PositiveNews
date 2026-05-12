namespace PositiveNews.Infrastructure.Configuration;

/// <summary>
/// Strongly typed binding for the <c>IngestionSettings</c> configuration section (appsettings).
/// </summary>
public class IngestionSettingsConfig
{
    /// <summary>Shared rules applied across all feeds.</summary>
    public CommonIngestionConfig Common { get; set; } = new();

    /// <summary>
    /// Optional per-source overrides keyed by an arbitrary label (e.g. source nickname).
    /// </summary>
    public Dictionary<string, SourceIngestionConfig> Sources { get; set; } = new();
}

/// <summary>
/// Cross-cutting ingestion parameters: positivity lexicon, HTML cleaning, and validation rules.
/// </summary>
public class CommonIngestionConfig
{
    /// <summary>Lexicon and algorithm knobs for rule-based positivity scoring.</summary>
    public PositivityAnalizerKeyPhrasesConfig PositivityAnalizerKeyPhrases { get; set; } = new();

    /// <summary>Patterns and allowed tags for HTML sanitization during ingest.</summary>
    public CleanerRulesConfig CleanerRules { get; set; } = new();

    /// <summary>Validation rules applied to parsed RSS items.</summary>
    public FeedItemValidationRulesConfig FeedItemValidationRules { get; set; } = new();
}

/// <summary>
/// Configuration for the lexicon-based positivity analyzer (words, phrases, negation, intensifiers).
/// </summary>
public class PositivityAnalizerKeyPhrasesConfig
{
    /// <summary>Single-token cues counted as positive polarity.</summary>
    public List<string> PositiveWords { get; set; } = [];

    /// <summary>Single-token cues counted as negative polarity.</summary>
    public List<string> NegativeWords { get; set; } = [];

    /// <summary>Multi-word positive phrases (matched longest-first).</summary>
    public List<string> PositivePhrases { get; set; } = [];

    /// <summary>Multi-word negative phrases (matched longest-first).</summary>
    public List<string> NegativePhrases { get; set; } = [];

    /// <summary>Words that negate polarity of a subsequent cue within the lookback window.</summary>
    public List<string> NegationWords { get; set; } = [];

    /// <summary>Words that amplify the magnitude of the subsequent polarity cue.</summary>
    public List<string> IntensifierWords { get; set; } = [];

    /// <summary>How many prior tokens can flip polarity (odd count of negation cues = flip).</summary>
    public int NegationLookbackTokens { get; set; } = 4;

    /// <summary>How many prior tokens may stack intensifiers.</summary>
    public int IntensifierLookbackTokens { get; set; } = 2;

    /// <summary>Each intensifier in range multiplies weight by this factor (compounded).</summary>
    public double IntensifierMultiplier { get; set; } = 1.35;

    /// <summary>Absolute weight for each phrase hit vs. 1.0 per word hit.</summary>
    public double PhrasePolarityWeight { get; set; } = 2.0;
}

/// <summary>
/// Declarative rules used by <see cref="PositiveNews.Infrastructure.Services.HtmlSanitizer"/> when stripping feed HTML.
/// </summary>
public class CleanerRulesConfig
{
    /// <summary>When inner text matches, traversal stops (e.g. boilerplate markers).</summary>
    public List<string> StopProcessingPatterns { get; set; } = [];

    /// <summary>Nodes whose text matches are removed entirely.</summary>
    public List<string> RemoveNodePatterns { get; set; } = [];

    /// <summary><c>div</c> elements whose class contains any of these substrings are skipped.</summary>
    public List<string> RemoveDivClassPatterns { get; set; } = [];

    /// <summary>Paragraphs matching these patterns are dropped.</summary>
    public List<string> ShouldRemoveParagraphPatterns { get; set; } = [];

    /// <summary>HTML tag names (lowercase) that may be emitted in sanitized output.</summary>
    public List<string> AllowedTags { get; set; } = [];

    /// <summary>Attribute names stripped from retained elements.</summary>
    public List<string> AttributesToRemove { get; set; } = [];
}

/// <summary>
/// RSS item validation rules (e.g. blocked author names).
/// </summary>
public class FeedItemValidationRulesConfig
{
    /// <summary>Author strings that cause an item to be rejected.</summary>
    public List<string> InvalidAuthors { get; set; } = [];
}

/// <summary>
/// Optional per-source ingestion hints when the feed URL matches <see cref="UrlContains"/>.
/// </summary>
public class SourceIngestionConfig
{
    /// <summary>Substring matched case-insensitively against the feed URL to apply defaults.</summary>
    public string UrlContains { get; set; } = string.Empty;

    /// <summary>Topic names prepended or merged when this source matches.</summary>
    public List<string> DefaultTopics { get; set; } = [];
}
