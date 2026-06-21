using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class CommentWriteRepository(AppDbContext db) : ICommentWriteRepository
{
    /// <inheritdoc />
    public void Add(Comment comment) => db.Comments.Add(comment);

    /// <inheritdoc />
    public Task<Comment?> GetByIdAsync(long commentId, CancellationToken cancellationToken = default)
        => db.Comments.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
}