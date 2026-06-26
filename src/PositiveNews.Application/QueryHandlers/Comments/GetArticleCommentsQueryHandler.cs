using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Comments;
using PositiveNews.Application.Queries.Comments;

namespace PositiveNews.Application.QueryHandlers.Comments;

/// <summary>
/// Returns active top-level comments for an article when the article exists.
/// </summary>
public sealed class GetArticleCommentsQueryHandler(
    IArticleReadRepository articleReadRepository,
    ICommentReadRepository commentReadRepository)
    : IRequestHandler<GetArticleCommentsQuery, Result<IReadOnlyList<CommentListItemDto>>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<CommentListItemDto>>> Handle(
        GetArticleCommentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await articleReadRepository.ExistsActiveAsync(request.ArticleId, cancellationToken))
        {
            return Result<IReadOnlyList<CommentListItemDto>>.Failure(
                new Error("Article.NotFound", $"Article with id '{request.ArticleId}' was not found.", ErrorType.NotFound));
        }

        var comments = await commentReadRepository.GetActiveTopLevelByArticleIdAsync(request.ArticleId, cancellationToken);
        return Result<IReadOnlyList<CommentListItemDto>>.Success(comments);
    }
}
