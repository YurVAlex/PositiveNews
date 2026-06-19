using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class CommentWriteRepository(AppDbContext db) : ICommentWriteRepository
{
    /// <inheritdoc />
    public void Add(Comment comment) => db.Comments.Add(comment);
}
