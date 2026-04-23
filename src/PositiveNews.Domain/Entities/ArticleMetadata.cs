using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

public class ArticleMetadata
{
    private readonly List<ArticleTopic> _articleTopics = [];
    private readonly List<Comment> _comments = [];

    // For EF Core materialization
    private ArticleMetadata() { }

    public long Id { get; private set; }
    public int SourceId { get; private set; }
    public string? ExternalId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Author { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? ImageTag { get; private set; }
    public DateTime PublishedAt { get; private set; }
    public DateTime IngestedAt { get; private set; }
    public DateTime? AnalyzedAt { get; private set; }
    public decimal? PositivityScore { get; private set; }
    public long ViewCount { get; private set; }
    public string LanguageCode { get; private set; } = "und";
    public string RegionCode { get; private set; } = "Global";
    public bool IsActive { get; private set; } = true;
    public long? ModeratedBy { get; private set; }
    public string? SummaryShort { get; private set; }

    // Navigation
    public Source Source { get; private set; } = null!;
    public User? Moderator { get; private set; }
    public ArticleContent? Content { get; private set; }

    public IReadOnlyCollection<ArticleTopic> ArticleTopics => _articleTopics.AsReadOnly();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

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

    public void AttachContent(ArticleContent content)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));
        if (Content is not null)
            throw new InvalidArticleStateException("Content is already attached to this article.");
        Content = content;
    }

    public void Deactivate(long moderatorId)
    {
        if (!IsActive)
            throw new InvalidArticleStateException("Article is already inactive.");
        IsActive = false;
        ModeratedBy = moderatorId;
    }

    public void AddTopic(int topicId)
    {
        if (_articleTopics.Any(at => at.TopicId == topicId))
            return;
        _articleTopics.Add(ArticleTopic.Create(this, topicId));
    }

    public void IncrementViewCount() => ViewCount++;
}
