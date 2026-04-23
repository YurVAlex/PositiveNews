using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.DTOs.Articles;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

public interface IArticleReadRepository
{
    Task<ArticleFeedPageResult> GetFeedPageAsync(ArticleFeedFilter filter, CancellationToken ct);
    Task<ArticleDetailDto?> GetDetailAsync(long id, CancellationToken ct);
    Task<ExistingArticleKeys> FindExistingKeysAsync(
        IReadOnlyCollection<string?> externalIds,
        IReadOnlyCollection<string> urls,
        IReadOnlyCollection<string> titles,
        CancellationToken ct);
}
