using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
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
}
