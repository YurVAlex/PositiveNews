using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Write access for persisting comments.
/// </summary>
public interface ICommentWriteRepository
{
    /// <summary>
    /// Stages a new comment for persistence.
    /// </summary>
    /// <param name="comment">Comment entity to add.</param>
    void Add(Comment comment);

    /// <summary>
    /// Loads a comment by id for updates.
    /// </summary>
    /// <param name="commentId">Comment primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Comment?> GetByIdAsync(long commentId, CancellationToken cancellationToken = default);
}
