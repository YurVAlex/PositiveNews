using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

internal sealed class UserWriteRepository(AppDbContext db) : IUserWriteRepository
{
    public void Add(User user) => db.Users.Add(user);
}
