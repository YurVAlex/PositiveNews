using System.Reflection;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.TestSupport;

internal static class UserTestHelpers
{
    /// <summary>Adds a role to a user for tests (mirrors EF-loaded navigations).</summary>
    public static void AddRole(User user, Role role)
    {
        var userRole = UserRole.Create(0, user);
        typeof(UserRole).GetProperty(nameof(UserRole.Role))!.SetValue(userRole, role);
        var field = typeof(User).GetField("_userRoles", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var roles = (List<UserRole>)field.GetValue(user)!;
        roles.Add(userRole);
    }
}
