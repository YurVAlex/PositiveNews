using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;

namespace PositiveNews.Application.QueryHandlers.Admin;

/// <summary>
/// Loads a single article for administrative moderation.
/// </summary>
public sealed class GetAdminArticleDetailQueryHandler(IArticleReadRepository articleReadRepository)
    : IRequestHandler<GetAdminArticleDetailQuery, Result<ArticleAdminDetailDto>>
{
    public async Task<Result<ArticleAdminDetailDto>> Handle(
        GetAdminArticleDetailQuery request,
        CancellationToken cancellationToken)
    {
        var article = await articleReadRepository.GetAdminArticleDetailAsync(request.ArticleId, cancellationToken);
        if (article is null)
        {
            return Result<ArticleAdminDetailDto>.Failure(
                new Error(
                    ErrorCodes.Admin.ArticleNotFound,
                    $"Article with id '{request.ArticleId}' was not found.",
                    ErrorType.NotFound));
        }

        return Result<ArticleAdminDetailDto>.Success(article);
    }
}
