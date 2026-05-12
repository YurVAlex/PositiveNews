using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class UserRoleWriteRepository(AppDbContext db) : IUserRoleWriteRepository
{
    /// <inheritdoc />
    public void Add(UserRole userRole) => db.UserRoles.Add(userRole);
}
