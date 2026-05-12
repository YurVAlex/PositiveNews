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
}
