using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

internal sealed class ArticleTopicWriteRepository(AppDbContext db) : IArticleTopicWriteRepository
{
    public void AddTopicToArticle(long articleId, int topicId)
    {
        db.ArticleTopics.Add(ArticleTopic.Create(articleId, topicId));
    }
}
