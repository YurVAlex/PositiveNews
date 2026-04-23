namespace PositiveNews.Domain.Entities;

public class ArticleTopic
{
    // For EF Core materialization
    private ArticleTopic() { }

    public long ArticleId { get; private set; }
    public int TopicId { get; private set; }

    // Navigation
    public ArticleMetadata Article { get; private set; } = null!;
    public Topic Topic { get; private set; } = null!;

    public static ArticleTopic Create(long articleId, int topicId)
    {
        return new ArticleTopic { ArticleId = articleId, TopicId = topicId };
    }

    public static ArticleTopic Create(ArticleMetadata article, int topicId)
    {
        if (article is null) throw new ArgumentNullException(nameof(article));
        return new ArticleTopic
        {
            Article = article,
            TopicId = topicId
        };
    }
}
