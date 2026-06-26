using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Creates and updates user accounts.
/// </summary>
public interface IUserWriteRepository
{
    /// <summary>
    /// Stages a new user for insertion on commit.
    /// </summary>
    /// <param name="user">User aggregate root.</param>
    void Add(User user);

    /// <summary>
    /// Loads a user for update using tracking semantics.
    /// </summary>
    Task<User?> GetByIdAsync(long userId, CancellationToken ct);
}
