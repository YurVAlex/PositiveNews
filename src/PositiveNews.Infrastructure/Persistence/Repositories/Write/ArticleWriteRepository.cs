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

    /// <inheritdoc />
    public Task<ArticleMetadata?> GetByIdAsync(long articleId, CancellationToken cancellationToken = default)
        => db.ArticlesMetadata
            .Include(a => a.Content)
            .FirstOrDefaultAsync(a => a.Id == articleId, cancellationToken);

    public async Task DeactivateBySourceAsync(int sourceId, long moderatorId, CancellationToken cancellationToken = default)
    {
        var articles = await db.ArticlesMetadata
            .Where(a => a.SourceId == sourceId && a.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var article in articles)
        {
            if (article.IsActive)
            {
                article.Deactivate(moderatorId);
            }
        }
    }

    public async Task ActivateBySourceAsync(int sourceId, long moderatorId, CancellationToken cancellationToken = default)
    {
        var articles = await db.ArticlesMetadata
            .Where(a => a.SourceId == sourceId && !a.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var article in articles)
        {
            if (!article.IsActive)
            {
                article.Activate(moderatorId);
            }
        }
    }
}
