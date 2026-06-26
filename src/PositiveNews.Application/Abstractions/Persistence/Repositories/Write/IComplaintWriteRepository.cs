using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Write access for persisting comment complaints.
/// </summary>
public interface IComplaintWriteRepository
{
    /// <summary>
    /// Stages a new complaint for persistence.
    /// </summary>
    /// <param name="complaint">Complaint entity to add.</param>
    void Add(Complaint complaint);

    /// <summary>
    /// Checks whether the user already filed a complaint against the comment.
    /// </summary>
    /// <param name="userId">Complainant user id.</param>
    /// <param name="commentId">Comment primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> ExistsForUserAndCommentAsync(
        long userId,
        long commentId,
        CancellationToken cancellationToken = default);
}
