using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// Named security role (e.g. User, Admin) assigned to users via <see cref="UserRole"/>.
/// </summary>
public class Role
{
    private readonly List<UserRole> _userRoles = [];

    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private Role() { }

    /// <summary>Primary key.</summary>
    public int Id { get; private set; }

    /// <summary>Unique role name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Assignments of this role to users.</summary>
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    /// <summary>
    /// Creates a role with a trimmed, non-empty name.
    /// </summary>
    public static Role Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");
        return new Role { Name = name.Trim() };
    }
}
