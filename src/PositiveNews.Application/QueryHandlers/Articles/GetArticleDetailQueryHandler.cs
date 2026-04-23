using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

public sealed class GetArticleDetailQueryHandler(IArticleReadRepository articleReadRepository)
    : IRequestHandler<GetArticleDetailQuery, ArticleDetailDto?>
{
    public Task<ArticleDetailDto?> Handle(GetArticleDetailQuery request, CancellationToken cancellationToken)
    {
        return articleReadRepository.GetDetailAsync(request.Id, cancellationToken);
    }
}
