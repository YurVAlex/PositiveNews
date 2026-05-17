using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

/// <summary>
/// Retrieves article detail for public reading views with source branding fields.
/// </summary>
/// <param name="articleReadRepository">Read model queries for articles.</param>
public sealed class GetArticleDetailQueryHandler(IArticleReadRepository articleReadRepository)
    : IRequestHandler<GetArticleDetailQuery, Result<ArticleDetailDto>>
{
    /// <summary>
    /// Loads the article by id or returns not-found when missing.
    /// </summary>
    /// <param name="request">Article identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detail DTO or not-found error.</returns>
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
