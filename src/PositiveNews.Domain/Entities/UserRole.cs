namespace PositiveNews.Domain.Entities;

public class UserRole
{
    public long UserId { get; set; }
    public int RoleId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}