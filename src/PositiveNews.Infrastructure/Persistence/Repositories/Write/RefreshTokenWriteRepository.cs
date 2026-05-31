using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class RefreshTokenWriteRepository(AppDbContext db) : IRefreshTokenWriteRepository
{
    /// <inheritdoc />
    public void Add(RefreshToken refreshToken) => db.RefreshTokens.Add(refreshToken);

    /// <inheritdoc />
    public void Update(RefreshToken refreshToken) => db.RefreshTokens.Update(refreshToken);

    /// <inheritdoc />
    public void Remove(RefreshToken refreshToken) => db.RefreshTokens.Remove(refreshToken);
}
