using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class ArticleWriteRepository(AppDbContext db) : IArticleWriteRepository
{
    /// <inheritdoc />
    public void Add(ArticleMetadata article) => db.ArticlesMetadata.Add(article);
}
