using Microsoft.EntityFrameworkCore;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence;

/// <summary>
/// EF surface used by ingestion queries/commands so handlers stay testable and persistence stays in Infrastructure.
/// </summary>
public interface IIngestionDbContext
{
    DbSet<Topic> Topics { get; }
    DbSet<Source> Sources { get; }
    DbSet<ArticleMetadata> ArticlesMetadata { get; }
    DbSet<ArticleContent> ArticlesContent { get; }
    DbSet<ArticleTopic> ArticleTopics { get; }
    DbSet<IngestionRun> IngestionRuns { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
