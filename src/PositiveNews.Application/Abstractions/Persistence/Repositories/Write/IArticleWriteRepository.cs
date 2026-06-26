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

    /// <summary>
    /// Marks all articles for the source as inactive.
    /// </summary>
    /// <param name="sourceId">Source identifier.</param>
    /// <param name="moderatorId">Moderator identifier for the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeactivateBySourceAsync(int sourceId, long moderatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all articles for the source as active.
    /// </summary>
    /// <param name="sourceId">Source identifier.</param>
    /// <param name="moderatorId">Moderator identifier for the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ActivateBySourceAsync(int sourceId, long moderatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a single article for update using tracking semantics.
    /// </summary>
    /// <param name="articleId">Article identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ArticleMetadata?> GetByIdAsync(long articleId, CancellationToken cancellationToken = default);
}
