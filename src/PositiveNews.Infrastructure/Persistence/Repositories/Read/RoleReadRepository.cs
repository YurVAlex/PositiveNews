using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class RoleReadRepository(AppDbContext db) : IRoleReadRepository
{
    /// <inheritdoc />
    public Task<Role?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        => db.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
}
