using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class RefreshTokenReadRepository(AppDbContext db) : IRefreshTokenReadRepository
{
    /// <inheritdoc />
    public Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default)
        => db.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

    /// <inheritdoc />
    public Task<RefreshToken?> FindValidByTokenAsync(string token, CancellationToken cancellationToken = default)
        => db.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiresAtUtc > DateTime.UtcNow, cancellationToken);
}
