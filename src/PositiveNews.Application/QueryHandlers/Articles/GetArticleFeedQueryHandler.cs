using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

public sealed class GetArticleFeedQueryHandler(IArticleReadRepository articleReadRepository)
    : IRequestHandler<GetArticleFeedQuery, ArticleFeedPageResult>
{
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
