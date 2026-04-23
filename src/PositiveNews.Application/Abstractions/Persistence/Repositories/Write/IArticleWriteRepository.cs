using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

public interface IArticleWriteRepository
{
    void Add(ArticleMetadata article);
}
