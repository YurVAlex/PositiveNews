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
}
