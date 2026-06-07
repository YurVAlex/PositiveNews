using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// Aggregate root for an ingested news article: URLs, scoring, language, moderation state,
/// topic links, comments, and optional <see cref="ArticleContent"/>.
/// </summary>
public class ArticleMetadata
{
    private readonly List<ArticleTopic> _articleTopics = [];
    private readonly List<Comment> _comments = [];

    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private ArticleMetadata() { }

    /// <summary>Primary key.</summary>
    public long Id { get; private set; }

    /// <summary>Foreign key to the <see cref="Entities.Source"/> this article was ingested from.</summary>
    public int SourceId { get; private set; }

    /// <summary>Optional stable id from the external feed (e.g. RSS guid).</summary>
    public string? ExternalId { get; private set; }

    /// <summary>Article headline.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Optional author line from the feed.</summary>
    public string? Author { get; private set; }

    /// <summary>Canonical URL of the article.</summary>
    public string Url { get; private set; } = string.Empty;

    /// <summary>Optional preview image markup extracted from the feed.</summary>
    public string? ImageTag { get; private set; }

    /// <summary>Publication timestamp from the source.</summary>
    public DateTime PublishedAt { get; private set; }

    /// <summary>When this row was first stored.</summary>
    public DateTime IngestedAt { get; private set; }

    /// <summary>When sentiment analysis last ran, if applicable.</summary>
    public DateTime? AnalyzedAt { get; private set; }

    /// <summary>Lexicon-based positivity score in [0,1], or null if not analyzed.</summary>
    public decimal? PositivityScore { get; private set; }

    /// <summary>Number of detail views (incremented by the application).</summary>
    public long ViewCount { get; private set; }

    /// <summary>BCP 47 or ISO language tag (defaults to undetermined).</summary>
    public string LanguageCode { get; private set; } = "und";

    /// <summary>Region bucket for filtering (e.g. Global).</summary>
    public string RegionCode { get; private set; } = "Global";

    /// <summary>When false, the article is hidden from public feeds.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Moderator who last modified or deactivated the article, if any.</summary>
    public long? ModeratedBy { get; private set; }

    /// <summary>Short plain-text summary for cards and listings.</summary>
    public string? SummaryShort { get; private set; }

    /// <summary>Origin news source.</summary>
    public Source Source { get; private set; } = null!;

    /// <summary>Moderator navigation property when <see cref="ModeratedBy"/> is set.</summary>
    public User? Moderator { get; private set; }

    /// <summary>Optional article body (1-to-1).</summary>
    public ArticleContent? Content { get; private set; }

    /// <summary>Topics assigned to this article.</summary>
    public IReadOnlyCollection<ArticleTopic> ArticleTopics => _articleTopics.AsReadOnly();

    /// <summary>User comments on this article.</summary>
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    /// <summary>
    /// Validates input and creates a new article metadata instance for persistence.
    /// </summary>
    public static ArticleMetadata Create(
        int sourceId,
        string title,
        string url,
        string? externalId,
        DateTime publishedAt,
        string languageCode,
        decimal? positivityScore = null,
        string? author = null,
        string? summaryShort = null,
        string? imageTag = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidArticleStateException("Article title cannot be empty.");
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidArticleStateException("Article URL cannot be empty.");
        if (positivityScore.HasValue && (positivityScore.Value < 0m || positivityScore.Value > 1m))
            throw new InvalidArticleStateException(
                $"PositivityScore must be between 0 and 1 (got {positivityScore.Value}).");

        return new ArticleMetadata
        {
            SourceId = sourceId,
            Title = title.Length > 500 ? title[..500] : title.Trim(),
            Url = url.Trim(),
            ExternalId = externalId?.Trim(),
            PublishedAt = publishedAt,
            IngestedAt = DateTime.UtcNow,
            LanguageCode = string.IsNullOrWhiteSpace(languageCode) ? "und" : languageCode.Trim(),
            RegionCode = "Global",
            IsActive = true,
            PositivityScore = positivityScore,
            AnalyzedAt = positivityScore.HasValue ? DateTime.UtcNow : null,
            Author = author?.Trim(),
            SummaryShort = summaryShort,
            ImageTag = imageTag
        };
    }

    /// <summary>
    /// Links the one-to-one <see cref="ArticleContent"/> row; throws if content already exists.
    /// </summary>
    public void AttachContent(ArticleContent content)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));
        if (Content is not null)
            throw new InvalidArticleStateException("Content is already attached to this article.");
        Content = content;
    }

    /// <summary>
    /// Soft-deletes the article from feeds and records the acting moderator.
    /// </summary>
    public void Deactivate(long moderatorId)
    {
        if (!IsActive)
            throw new InvalidArticleStateException("Article is already inactive.");
        IsActive = false;
        ModeratedBy = moderatorId;
    }

    /// <summary>
    /// Reactivates a previously inactive article and records the acting moderator.
    /// </summary>
    public void Activate(long moderatorId)
    {
        if (IsActive)
            throw new InvalidArticleStateException("Article is already active.");
        IsActive = true;
        ModeratedBy = moderatorId;
    }

    /// <summary>
    /// Marks the article as modified by a moderator without changing its active state.
    /// </summary>
    public void ApplyModeration(long moderatorId)
    {
        if (moderatorId <= 0)
            throw new InvalidArticleStateException("ModeratorId must be a valid user identifier.");

        ModeratedBy = moderatorId;
    }

    /// <summary>
    /// Updates the article title for administrative corrections.
    /// </summary>
    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidArticleStateException("Article title cannot be empty.");

        Title = title.Length > 500 ? title[..500] : title.Trim();
    }

    /// <summary>
    /// Updates the preview image markup.
    /// </summary>
    public void UpdateImageTag(string? imageTag)
    {
        ImageTag = string.IsNullOrWhiteSpace(imageTag) ? null : imageTag.Trim();
    }

    /// <summary>
    /// Updates the article positivity score.
    /// </summary>
    public void UpdatePositivityScore(decimal? positivityScore)
    {
        if (positivityScore.HasValue && (positivityScore.Value < 0m || positivityScore.Value > 1m))
            throw new InvalidArticleStateException("PositivityScore must be between 0 and 1.");

        PositivityScore = positivityScore;
    }

    /// <summary>
    /// Updates the short summary shown in admin and listing cards.
    /// </summary>
    public void UpdateSummaryShort(string? summaryShort)
    {
        SummaryShort = string.IsNullOrWhiteSpace(summaryShort) ? null : summaryShort.Trim();
    }

    /// <summary>
    /// Adds a topic association if it is not already present.
    /// </summary>
    public void AddTopic(int topicId)
    {
        if (_articleTopics.Any(at => at.TopicId == topicId))
            return;
        _articleTopics.Add(ArticleTopic.Create(this, topicId));
    }

    /// <summary>Increments the view counter (typically when a user opens the detail page).</summary>
    public void IncrementViewCount() => ViewCount++;
}
