namespace PositiveNews.Domain.Entities;

public class UserRole
{
    // For EF Core materialization
    private UserRole() { }

    public long UserId { get; private set; }
    public int RoleId { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    public static UserRole Create(long userId, int roleId)
    {
        return new UserRole { UserId = userId, RoleId = roleId };
    }
}
