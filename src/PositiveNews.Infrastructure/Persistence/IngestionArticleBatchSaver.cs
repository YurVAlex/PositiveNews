using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence;

/// <summary>
/// Saves ingested article batches, falling back to per-item inserts when unique constraints race.
/// </summary>
public sealed class IngestionArticleBatchSaver(
    IIngestionUnitOfWork ingestionUnitOfWork,
    IArticleWriteRepository articleWriteRepository,
    ILogger<IngestionArticleBatchSaver> logger) : IIngestionArticleBatchSaver
{
    /// <inheritdoc />
    public async Task<int> SaveAddedArticlesAsync(
        IReadOnlyList<ArticleMetadata> articles,
        CancellationToken cancellationToken = default)
    {
        if (articles.Count == 0)
            return 0;

        try
        {
            await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);
            return articles.Count;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(
                ex,
                "Unique constraint violation while saving {Count} ingested articles; retrying individually.",
                articles.Count);
            ingestionUnitOfWork.ClearPendingChanges();
            return await SaveIndividuallySkippingDuplicatesAsync(articles, cancellationToken);
        }
    }

    private async Task<int> SaveIndividuallySkippingDuplicatesAsync(
        IReadOnlyList<ArticleMetadata> articles,
        CancellationToken cancellationToken)
    {
        var saved = 0;

        foreach (var article in articles)
        {
            articleWriteRepository.Add(article);

            try
            {
                await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);
                saved++;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                ingestionUnitOfWork.ClearPendingChanges();
                logger.LogWarning(
                    ex,
                    "Skipping duplicate ingested article with external id '{ExternalId}'.",
                    article.ExternalId);
            }
        }

        return saved;
    }

    internal static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sql && (sql.Number == 2627 || sql.Number == 2601))
                return true;
        }

        return false;
    }
}
