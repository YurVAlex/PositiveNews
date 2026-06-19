using PositiveNews.Application.DTOs.Comments;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

/// <summary>
/// Read-only access to article comments.
/// </summary>
public interface ICommentReadRepository
{
    /// <summary>
    /// Loads active top-level comments for an article ordered by creation time.
    /// </summary>
    /// <param name="articleId">Article primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<CommentListItemDto>> GetActiveTopLevelByArticleIdAsync(
        long articleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an active comment that belongs to the given article, or null when not found.
    /// </summary>
    /// <param name="commentId">Comment primary key.</param>
    /// <param name="articleId">Expected article id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ActiveCommentDto?> GetActiveByIdForArticleAsync(
        long commentId,
        long articleId,
        CancellationToken cancellationToken = default);
}
