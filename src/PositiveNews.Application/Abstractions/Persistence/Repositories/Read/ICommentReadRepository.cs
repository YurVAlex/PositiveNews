using PositiveNews.Application.DTOs.Admin;
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

    /// <summary>
    /// Loads comment detail for admin moderation, including complaints.
    /// </summary>
    /// <param name="commentId">Comment primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CommentAdminDetailDto?> GetAdminDetailByIdAsync(
        long commentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads active comments for admin list ordered by complaint count.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<CommentAdminItemDto>> GetAdminActiveCommentsAsync(
        CancellationToken cancellationToken = default);
}
