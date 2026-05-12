using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Tests.TestHelpers;

internal static class EntityBuilders
{
    public static User CreateUser(string email = "user@test.com", string name = "Test User")
        => User.Create(email, name);

    public static Role CreateRole(string name = "User")
        => Role.Create(name);
}
