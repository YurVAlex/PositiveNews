using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;

namespace PositiveNews.Application.QueryHandlers.Admin;

public sealed class GetAdminCommentDetailQueryHandler(ICommentReadRepository commentReadRepository)
    : IRequestHandler<GetAdminCommentDetailQuery, Result<CommentAdminDetailDto>>
{
    public async Task<Result<CommentAdminDetailDto>> Handle(
        GetAdminCommentDetailQuery request,
        CancellationToken cancellationToken)
    {
        var comment = await commentReadRepository.GetAdminDetailByIdAsync(request.CommentId, cancellationToken);
        if (comment is null)
        {
            return Result<CommentAdminDetailDto>.Failure(new Error(
                "Admin.CommentNotFound",
                $"Comment with id '{request.CommentId}' was not found.",
                ErrorType.NotFound));
        }

        return Result<CommentAdminDetailDto>.Success(comment);
    }
}
