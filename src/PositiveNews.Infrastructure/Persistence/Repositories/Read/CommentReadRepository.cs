using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.DTOs.Comments;
using PositiveNews.Infrastructure.Persistence;
namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class CommentReadRepository(AppDbContext db) : ICommentReadRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<CommentListItemDto>> GetActiveTopLevelByArticleIdAsync(
        long articleId,
        CancellationToken cancellationToken = default)
    {
        return await db.Comments
            .AsNoTracking()
            .Where(c => c.ArticleId == articleId && c.ParentId == null && c.IsActive)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentListItemDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.User.Name,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<ActiveCommentDto?> GetActiveByIdForArticleAsync(
        long commentId,
        long articleId,
        CancellationToken cancellationToken = default)
    {
        return db.Comments
            .AsNoTracking()
            .Where(c => c.Id == commentId && c.ArticleId == articleId && c.IsActive)
            .Select(c => new ActiveCommentDto
            {
                Id = c.Id,
                UserId = c.UserId,
                ArticleId = c.ArticleId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<CommentAdminDetailDto?> GetAdminDetailByIdAsync(
        long commentId,
        CancellationToken cancellationToken = default)
    {
        return db.Comments
            .AsNoTracking()
            .Where(c => c.Id == commentId)
            .Select(c => new CommentAdminDetailDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                UserName = c.User.Name,
                IsActive = c.IsActive,
                ModeratedBy = c.ModeratedBy,
                ArticleId = c.ArticleId,
                Complaints = c.Complaints
                    .OrderBy(complaint => complaint.CreatedAt)
                    .Select(complaint => new CommentComplaintAdminItemDto
                    {
                        Id = complaint.Id,
                        UserId = complaint.UserId,
                        UserName = complaint.User.Name,
                        Reason = complaint.Reason,
                        CreatedAt = complaint.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
