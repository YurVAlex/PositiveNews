using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

internal sealed class UserRoleWriteRepository(AppDbContext db) : IUserRoleWriteRepository
{
    public void Add(UserRole userRole) => db.UserRoles.Add(userRole);
}
