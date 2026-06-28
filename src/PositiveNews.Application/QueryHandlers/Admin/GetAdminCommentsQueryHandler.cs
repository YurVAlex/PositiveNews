using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;

namespace PositiveNews.Application.QueryHandlers.Admin;

public sealed class GetAdminCommentsQueryHandler(ICommentReadRepository commentReadRepository)
    : IRequestHandler<GetAdminCommentsQuery, Result<IReadOnlyList<CommentAdminItemDto>>>
{
    public async Task<Result<IReadOnlyList<CommentAdminItemDto>>> Handle(
        GetAdminCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await commentReadRepository.GetAdminActiveCommentsAsync(cancellationToken);
        return Result<IReadOnlyList<CommentAdminItemDto>>.Success(items);
    }
}
