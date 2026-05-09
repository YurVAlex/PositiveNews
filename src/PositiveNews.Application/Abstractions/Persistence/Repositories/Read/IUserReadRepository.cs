using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

public interface IUserReadRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> FindByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> FindByIdWithRolesAsync(long id, CancellationToken cancellationToken = default);
}
