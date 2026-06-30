using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Commands.Comments;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Comments;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.CommandHandlers.Comments;

/// <summary>
/// Persists a new top-level comment on an active article.
/// </summary>
public sealed class AddArticleCommentCommandHandler(
    IArticleReadRepository articleReadRepository,
    IUserReadRepository userReadRepository,
    ICommentWriteRepository commentWriteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddArticleCommentCommand, Result<CommentCreatedDto>>
{
    /// <inheritdoc />
    public async Task<Result<CommentCreatedDto>> Handle(
        AddArticleCommentCommand request,
        CancellationToken cancellationToken)
    {
        if (!await articleReadRepository.ExistsActiveAsync(request.ArticleId, cancellationToken))
        {
            return Result<CommentCreatedDto>.Failure(
                new Error(ErrorCodes.Article.NotFound, $"Article with id '{request.ArticleId}' was not found.", ErrorType.NotFound));
        }

        var user = await userReadRepository.FindByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<CommentCreatedDto>.Failure(
                new Error(ErrorCodes.User.NotFound, $"User with id '{request.UserId}' was not found.", ErrorType.NotFound));
        }

        var comment = Comment.Create(request.ArticleId, request.UserId, request.Content);
        commentWriteRepository.Add(comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CommentCreatedDto>.Success(new CommentCreatedDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            UserName = user.Name,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        });
    }
}
