using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence;

/// <summary>
/// Persists staged ingestion articles with duplicate-key tolerance for race conditions.
/// </summary>
public interface IIngestionArticleBatchSaver
{
    /// <summary>
    /// Commits articles already staged on the ingestion unit of work.
    /// </summary>
    /// <param name="articles">Articles previously added via <see cref="Repositories.Write.IArticleWriteRepository.Add"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of articles actually saved (duplicates are skipped).</returns>
    Task<int> SaveAddedArticlesAsync(
        IReadOnlyList<ArticleMetadata> articles,
        CancellationToken cancellationToken = default);
}
