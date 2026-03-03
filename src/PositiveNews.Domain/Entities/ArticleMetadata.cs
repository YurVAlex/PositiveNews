using System.Xml.Linq;

namespace PositiveNews.Domain.Entities;

public class ArticleMetadata
{
    public long Id { get; set; }
    public int SourceId { get; set; }
    public string? ExternalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime IngestedAt { get; set; }
    public DateTime? AnalyzedAt { get; set; }
    public decimal? PositivityScore { get; set; }
    public long ViewCount { get; set; }
    public string LanguageCode { get; set; } = "und";
    public string RegionCode { get; set; } = "Global";
    public bool IsActive { get; set; } = true;
    public long? ModeratedBy { get; set; }

    // Navigation
    public Source Source { get; set; } = null!;
    public User? Moderator { get; set; }
    public ArticleContent? Content { get; set; }
    public ICollection<ArticleTopic> ArticleTopics { get; set; } = new List<ArticleTopic>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}