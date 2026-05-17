using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

/// <summary>
/// Read-only access to user accounts and role assignments.
/// </summary>
public interface IUserReadRepository
{
    /// <summary>
    /// Checks whether an account already exists for the email address.
    /// </summary>
    /// <param name="email">Normalized or raw email (caller convention).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when a user with this email exists.</returns>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a user with roles by email for authentication flows.
    /// </summary>
    /// <param name="email">Email address to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user with roles, or <see langword="null"/>.</returns>
    Task<User?> FindByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a user with roles by primary key (e.g. current-user profile).
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user with roles, or <see langword="null"/>.</returns>
    Task<User?> FindByIdWithRolesAsync(long id, CancellationToken cancellationToken = default);
}
