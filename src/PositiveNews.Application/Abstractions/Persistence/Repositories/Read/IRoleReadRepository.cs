using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

public interface IRoleReadRepository
{
    Task<Role?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
}
