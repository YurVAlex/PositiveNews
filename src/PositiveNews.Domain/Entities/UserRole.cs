namespace PositiveNews.Domain.Entities;

/// <summary>
/// Join entity assigning a <see cref="Role"/> to a <see cref="User"/>.
/// </summary>
public class UserRole
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private UserRole() { }

    /// <summary>User receiving the role.</summary>
    public long UserId { get; private set; }

    /// <summary>Role granted.</summary>
    public int RoleId { get; private set; }

    /// <summary>Navigation to the user.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Navigation to the role.</summary>
    public Role Role { get; private set; } = null!;

    /// <summary>
    /// Creates an assignment by foreign keys (when both ids are known).
    /// </summary>
    public static UserRole Create(long userId, int roleId)
    {
        return new UserRole { UserId = userId, RoleId = roleId };
    }

    /// <summary>
    /// Creates an assignment while attaching the user navigation (common on registration).
    /// </summary>
    public static UserRole Create(int roleId, User user)
    {
        return new UserRole { User = user, RoleId = roleId };
    }
}
