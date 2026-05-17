using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Assigns roles to users.
/// </summary>
public interface IUserRoleWriteRepository
{
    /// <summary>
    /// Stages a user-role link for insertion on commit.
    /// </summary>
    /// <param name="userRole">Join entity linking user and role.</param>
    void Add(UserRole userRole);
}
