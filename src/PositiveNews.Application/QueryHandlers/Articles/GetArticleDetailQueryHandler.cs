using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

public sealed class GetArticleDetailQueryHandler(IArticleReadRepository articleReadRepository)
    : IRequestHandler<GetArticleDetailQuery, Result<ArticleDetailDto>>
{
    public async Task<Result<ArticleDetailDto>> Handle(GetArticleDetailQuery request, CancellationToken cancellationToken)
    {
        var article = await articleReadRepository.GetDetailAsync(request.Id, cancellationToken);
        if (article is null)
        {
            return Result<ArticleDetailDto>.Failure(
                new Error("Article.NotFound", $"Article with id '{request.Id}' was not found.", ErrorType.NotFound));
        }

        return Result<ArticleDetailDto>.Success(article);
    }
}
