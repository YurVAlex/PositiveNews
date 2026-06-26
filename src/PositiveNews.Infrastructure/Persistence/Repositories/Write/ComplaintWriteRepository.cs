using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class ComplaintWriteRepository(AppDbContext db) : IComplaintWriteRepository
{
    /// <inheritdoc />
    public void Add(Complaint complaint) => db.Complains.Add(complaint);

    /// <inheritdoc />
    public Task<bool> ExistsForUserAndCommentAsync(
        long userId,
        long commentId,
        CancellationToken cancellationToken = default)
        => db.Complains.AnyAsync(c => c.UserId == userId && c.CommentId == commentId, cancellationToken);
}
