using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;

namespace PositiveNews.Application.QueryHandlers.Admin;

/// <summary>
/// Searches articles for administrative moderation.
/// </summary>
public sealed class GetAdminArticlesQueryHandler(IArticleReadRepository articleReadRepository)
    : IRequestHandler<GetAdminArticlesQuery, Result<IReadOnlyList<ArticleAdminItemDto>>>
{
    public async Task<Result<IReadOnlyList<ArticleAdminItemDto>>> Handle(
        GetAdminArticlesQuery request,
        CancellationToken cancellationToken)
    {
        var results = await articleReadRepository.SearchAdminArticlesAsync(request.SearchTerm, cancellationToken);
        return Result<IReadOnlyList<ArticleAdminItemDto>>.Success(results);
    }
}
