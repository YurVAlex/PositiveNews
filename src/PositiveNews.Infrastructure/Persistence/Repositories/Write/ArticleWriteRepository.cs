using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class ArticleWriteRepository(AppDbContext db) : IArticleWriteRepository
{
    /// <inheritdoc />
    public void Add(ArticleMetadata article) => db.ArticlesMetadata.Add(article);

    /// <inheritdoc />
    public async Task<bool> TryIncrementViewCountAsync(long id, CancellationToken cancellationToken = default)
    {
        var article = await db.ArticlesMetadata
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (article is null)
        {
            return false;
        }

        article.IncrementViewCount();
        return true;
    }
}
