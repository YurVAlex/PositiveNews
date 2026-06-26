using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// A news origin: branding, feed URL, trust metadata, and ingestion history.
/// </summary>
public class Source
{
    private readonly List<ArticleMetadata> _articles = [];
    private readonly List<IngestionRun> _ingestionRuns = [];
    private readonly List<UserSourceFilter> _userSourceFilters = [];

    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private Source() { }

    /// <summary>Primary key.</summary>
    public int Id { get; private set; }

    /// <summary>Display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional longer description.</summary>
    public string? Description { get; private set; }

    /// <summary>Site root URL.</summary>
    public string BaseUrl { get; private set; } = string.Empty;

    /// <summary>RSS or Atom feed URL used by ingestion.</summary>
    public string? FeedUrl { get; private set; }

    /// <summary>Optional logo image URL.</summary>
    public string? LogoUrl { get; private set; }

    /// <summary>Optional partner API endpoint.</summary>
    public string? ApiEndpoint { get; private set; }

    /// <summary>Stored API key material (encrypted at rest by infrastructure).</summary>
    public string? ApiEncryptedKey { get; private set; }

    /// <summary>Editorial trust weight (non-negative).</summary>
    public decimal TrustScore { get; private set; } = 1.0m;

    /// <summary>Default BCP 47 language tag for articles from this source.</summary>
    public string DefaultLanguageCode { get; private set; } = "en";

    /// <summary>Optional HTML snippet for default thumbnails when the feed lacks images.</summary>
    public string? DefaultThumbnailHtml { get; private set; }

    /// <summary>When this source row was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>When false, ingestion and UI should ignore this source.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Moderator who deactivated the source, if any.</summary>
    public long? ModeratedBy { get; private set; }

    /// <summary>Moderator navigation.</summary>
    public User? Moderator { get; private set; }

    /// <summary>Articles ingested from this source.</summary>
    public IReadOnlyCollection<ArticleMetadata> Articles => _articles.AsReadOnly();

    /// <summary>Historical ingestion runs.</summary>
    public IReadOnlyCollection<IngestionRun> IngestionRuns => _ingestionRuns.AsReadOnly();

    /// <summary>Users who filter their feed to this source.</summary>
    public IReadOnlyCollection<UserSourceFilter> UserSourceFilters => _userSourceFilters.AsReadOnly();

    /// <summary>
    /// Validates input and creates a new active source.
    /// </summary>
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

    /// <summary>
    /// Soft-deactivates the source and records the moderator.
    /// </summary>
    public void Deactivate(long moderatorId)
    {
        if (!IsActive)
            throw new InvalidSourceStateException("Source is already inactive.");
        IsActive = false;
        ModeratedBy = moderatorId;
    }

    /// <summary>
    /// Updates the feed URL used by background ingestion.
    /// </summary>
    public void UpdateFeedUrl(string newFeedUrl)
    {
        if (string.IsNullOrWhiteSpace(newFeedUrl))
            throw new InvalidSourceStateException("Feed URL cannot be empty.");
        FeedUrl = newFeedUrl.Trim();
    }

    /// <summary>
    /// Updates the source's trust score.
    /// </summary>
    public void SetTrustScore(decimal trustScore, long moderatorId)
    {
        if (trustScore < 0m)
            throw new InvalidSourceStateException("TrustScore cannot be negative.");

        TrustScore = trustScore;
        ModeratedBy = moderatorId;
    }

    /// <summary>
    /// Activates or deactivates the source and records moderation metadata.
    /// </summary>
    public void SetActive(bool isActive, long moderatorId)
    {
        if (IsActive == isActive)
        {
            ModeratedBy = moderatorId;
            return;
        }

        IsActive = isActive;
        ModeratedBy = moderatorId;
    }

    /// <summary>
    /// Marks the source as modified by a moderator without changing other fields.
    /// </summary>
    public void ApplyModeration(long moderatorId)
    {
        if (moderatorId <= 0)
            throw new InvalidSourceStateException("ModeratorId must be a valid user identifier.");

        ModeratedBy = moderatorId;
    }

    /// <summary>
    /// Updates marketing/description fields without touching feed configuration.
    /// </summary>
    public void UpdateDetails(string? description, string? logoUrl)
    {
        Description = description;
        LogoUrl = logoUrl?.Trim();
    }
}
