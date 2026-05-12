using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

/// <summary>
/// Read-only access to security roles.
/// </summary>
public interface IRoleReadRepository
{
    /// <summary>
    /// Finds a role by its unique name (case-sensitive per storage rules).
    /// </summary>
    /// <param name="name">Role name (e.g. User, Admin).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The role entity, or <see langword="null"/> when missing.</returns>
    Task<Role?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
}
