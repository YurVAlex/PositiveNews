namespace PositiveNews.Domain.Entities;

public class ArticleTopic
{
    public long ArticleId { get; set; }
    public int TopicId { get; set; }

    // Navigation
    public ArticleMetadata Article { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
}