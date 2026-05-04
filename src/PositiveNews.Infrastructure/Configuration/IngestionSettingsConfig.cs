namespace PositiveNews.Infrastructure.Configuration;

public class IngestionSettingsConfig
{
    public CommonIngestionConfig Common { get; set; } = new();
    public Dictionary<string, SourceIngestionConfig> Sources { get; set; } = new();
}

public class CommonIngestionConfig
{
    public PositivityAnalizerKeyPhrasesConfig PositivityAnalizerKeyPhrases { get; set; } = new();
    public CleanerRulesConfig CleanerRules { get; set; } = new();
    public FeedItemValidationRulesConfig FeedItemValidationRules { get; set; } = new();
}

public class PositivityAnalizerKeyPhrasesConfig
{
    public List<string> PositiveWords { get; set; } = [];
    public List<string> NegativeWords { get; set; } = [];
    public List<string> PositivePhrases { get; set; } = [];
    public List<string> NegativePhrases { get; set; } = [];
    public List<string> NegationWords { get; set; } = [];
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

public class CleanerRulesConfig
{
    public List<string> StopProcessingPatterns { get; set; } = [];
    public List<string> RemoveNodePatterns { get; set; } = [];
    public List<string> RemoveDivClassPatterns { get; set; } = [];
    public List<string> ShouldRemoveParagraphPatterns { get; set; } = [];
    public List<string> AllowedTags { get; set; } = [];
    public List<string> AttributesToRemove { get; set; } = [];
}

public class FeedItemValidationRulesConfig
{
    public List<string> InvalidAuthors { get; set; } = [];
}

public class SourceIngestionConfig
{
    public string UrlContains { get; set; } = string.Empty;
    public List<string> DefaultTopics { get; set; } = [];
}
