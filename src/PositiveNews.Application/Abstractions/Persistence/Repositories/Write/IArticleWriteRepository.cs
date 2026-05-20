using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Adds new article metadata rows during ingestion.
/// </summary>
public interface IArticleWriteRepository
{
    /// <summary>
    /// Stages a new article for insertion on the next unit-of-work commit.
    /// </summary>
    /// <param name="article">Article metadata aggregate root.</param>
    void Add(ArticleMetadata article);

    /// <summary>
    /// Increments <see cref="ArticleMetadata.ViewCount"/> for an active article when its detail page is opened.
    /// </summary>
    /// <param name="id">Article identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when the article exists and was updated; otherwise <c>false</c>.</returns>
    Task<bool> TryIncrementViewCountAsync(long id, CancellationToken cancellationToken = default);
}
