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
