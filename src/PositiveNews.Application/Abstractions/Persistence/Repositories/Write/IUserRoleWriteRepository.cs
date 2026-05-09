using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

public interface IUserRoleWriteRepository
{
    void Add(UserRole userRole);
}
