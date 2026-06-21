using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class UserReadRepository(AppDbContext db) : IUserReadRepository
{
    /// <inheritdoc />
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => db.Users.AnyAsync(u => u.Email == email, cancellationToken);

    /// <inheritdoc />
    public Task<User?> FindByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
        => db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    /// <inheritdoc />
    public Task<User?> FindByIdWithRolesAsync(long id, CancellationToken cancellationToken = default)
        => db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserAdminItemDto>> SearchAdminUsersAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var trimmed = searchTerm.Trim();
            var isIdSearch = long.TryParse(trimmed, out var userId);
            query = query.Where(u => (isIdSearch && u.Id == userId) || u.Name.Contains(trimmed));
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Take(50)
            .Select(u => new UserAdminItemDto
            {
                Id = u.Id,
                Name = u.Name,
                IsActive = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                FailedLoginCount = u.FailedLoginCount,
                CreatedAt = u.CreatedAt,
                ModeratedBy = u.ModeratedBy
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserAdminDetailDto?> GetAdminUserDetailAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserAdminDetailDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                IsActive = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                FailedLoginCount = u.FailedLoginCount,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                ModeratedBy = u.ModeratedBy
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
