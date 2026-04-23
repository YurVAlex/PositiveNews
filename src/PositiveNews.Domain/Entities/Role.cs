using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

public class Role
{
    private readonly List<UserRole> _userRoles = [];

    // For EF Core materialization
    private Role() { }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // Navigation
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public static Role Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");
        return new Role { Name = name.Trim() };
    }
}
