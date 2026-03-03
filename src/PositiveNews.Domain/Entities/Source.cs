namespace PositiveNews.Domain.Entities;

public class Source
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string? FeedUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? ApiEndpoint { get; set; }
    public string? ApiEncryptedKey { get; set; }
    public decimal TrustScore { get; set; } = 1.0m;
    public string DefaultLanguageCode { get; set; } = "en";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public long? ModeratedBy { get; set; }

    // Navigation
    public User? Moderator { get; set; }
    public ICollection<ArticleMetadata> Articles { get; set; } = new List<ArticleMetadata>();
    public ICollection<IngestionRun> IngestionRuns { get; set; } = new List<IngestionRun>();
    public ICollection<UserSourceFilter> UserSourceFilters { get; set; } = new List<UserSourceFilter>();
}