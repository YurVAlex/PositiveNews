using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

/// <summary>
/// Projects repository feed queries using paging, topic filters, and sort options from the query.
/// </summary>
/// <param name="articleReadRepository">Paged article feed access.</param>
public sealed class GetArticleFeedQueryHandler(IArticleReadRepository articleReadRepository)
    : IRequestHandler<GetArticleFeedQuery, ArticleFeedPageResult>
{
    /// <summary>
    /// Maps the MediatR query into an <see cref="PositiveNews.Application.Abstractions.Persistence.Models.ArticleFeedFilter"/> and loads one page.
    /// </summary>
    /// <param name="request">Paging and filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged feed result for the UI.</returns>
    public Task<ArticleFeedPageResult> Handle(GetArticleFeedQuery request, CancellationToken cancellationToken)
    {
        var filter = new ArticleFeedFilter(
            request.Page,
            request.PageSize,
            request.Topics ?? Array.Empty<string>(),
            request.SortBy);
        return articleReadRepository.GetFeedPageAsync(filter, cancellationToken);
    }
}
