namespace PositiveNews.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}