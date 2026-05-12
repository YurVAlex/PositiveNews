namespace PositiveNews.Domain.Entities;

/// <summary>
/// Join entity linking an <see cref="ArticleMetadata"/> row to a <see cref="Topic"/>.
/// </summary>
public class ArticleTopic
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private ArticleTopic() { }

    /// <summary>Foreign key to the article.</summary>
    public long ArticleId { get; private set; }

    /// <summary>Foreign key to the topic.</summary>
    public int TopicId { get; private set; }

    /// <summary>Navigation to the article.</summary>
    public ArticleMetadata Article { get; private set; } = null!;

    /// <summary>Navigation to the topic.</summary>
    public Topic Topic { get; private set; } = null!;

    /// <summary>
    /// Creates an association by ids (e.g. after the article id is known).
    /// </summary>
    public static ArticleTopic Create(long articleId, int topicId)
    {
        return new ArticleTopic { ArticleId = articleId, TopicId = topicId };
    }

    /// <summary>
    /// Creates an association wiring the article navigation for EF inserts.
    /// </summary>
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
