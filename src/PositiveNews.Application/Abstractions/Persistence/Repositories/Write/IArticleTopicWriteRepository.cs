namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

public interface IArticleTopicWriteRepository
{
    void AddTopicToArticle(long articleId, int topicId);
}
