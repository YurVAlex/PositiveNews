using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class UserWriteRepository(AppDbContext db) : IUserWriteRepository
{
    /// <inheritdoc />
    public void Add(User user) => db.Users.Add(user);

    /// <inheritdoc />
    public Task<User?> GetByIdAsync(long userId, CancellationToken ct)
        => db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
}
