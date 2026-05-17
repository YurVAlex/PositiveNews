namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Tracks associations between articles and topics during ingestion or admin edits.
/// </summary>
public interface IArticleTopicWriteRepository
{
    /// <summary>
    /// Links an existing topic to an article (idempotent per persistence rules).
    /// </summary>
    /// <param name="articleId">Article primary key.</param>
    /// <param name="topicId">Topic identifier.</param>
    void AddTopicToArticle(long articleId, int topicId);
}
