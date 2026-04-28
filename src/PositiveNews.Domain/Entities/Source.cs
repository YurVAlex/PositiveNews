using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

public class Source
{
    private readonly List<ArticleMetadata> _articles = [];
    private readonly List<IngestionRun> _ingestionRuns = [];
    private readonly List<UserSourceFilter> _userSourceFilters = [];

    // For EF Core materialization
    private Source() { }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string BaseUrl { get; private set; } = string.Empty;
    public string? FeedUrl { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? ApiEndpoint { get; private set; }
    public string? ApiEncryptedKey { get; private set; }
    public decimal TrustScore { get; private set; } = 1.0m;
    public string DefaultLanguageCode { get; private set; } = "en";
    public string? DefaultThumbnailHtml { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public long? ModeratedBy { get; private set; }

    // Navigation
    public User? Moderator { get; private set; }
    public IReadOnlyCollection<ArticleMetadata> Articles => _articles.AsReadOnly();
    public IReadOnlyCollection<IngestionRun> IngestionRuns => _ingestionRuns.AsReadOnly();
    public IReadOnlyCollection<UserSourceFilter> UserSourceFilters => _userSourceFilters.AsReadOnly();

    public static Source Create(
        string name,
        string baseUrl,
        string? feedUrl = null,
        string? description = null,
        string? logoUrl = null,
        decimal trustScore = 1.0m,
        string defaultLanguageCode = "en",
        string? defaultThumbnailHtml = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidSourceStateException("Source name cannot be empty.");
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidSourceStateException("Source base URL cannot be empty.");
        if (trustScore < 0m)
            throw new InvalidSourceStateException("TrustScore cannot be negative.");

        return new Source
        {
            Name = name.Trim(),
            BaseUrl = baseUrl.Trim(),
            FeedUrl = feedUrl?.Trim(),
            Description = description,
            LogoUrl = logoUrl?.Trim(),
            TrustScore = trustScore,
            DefaultLanguageCode = string.IsNullOrWhiteSpace(defaultLanguageCode) ? "en" : defaultLanguageCode.Trim(),
            DefaultThumbnailHtml = defaultThumbnailHtml,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Deactivate(long moderatorId)
    {
        if (!IsActive)
            throw new InvalidSourceStateException("Source is already inactive.");
        IsActive = false;
        ModeratedBy = moderatorId;
    }

    public void UpdateFeedUrl(string newFeedUrl)
    {
        if (string.IsNullOrWhiteSpace(newFeedUrl))
            throw new InvalidSourceStateException("Feed URL cannot be empty.");
        FeedUrl = newFeedUrl.Trim();
    }

    public void UpdateDetails(string? description, string? logoUrl) 
    {
        Description = description;
        LogoUrl = logoUrl?.Trim();
    }
}
