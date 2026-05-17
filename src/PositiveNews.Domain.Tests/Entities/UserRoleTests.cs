using FluentAssertions;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Domain.Tests.Entities;

public class UserRoleTests
{
    [Fact]
    public void Create_Should_SetUserAndRoleIds_When_ByIds()
    {
        var ur = UserRole.Create(1, 2);

        ur.UserId.Should().Be(1);
        ur.RoleId.Should().Be(2);
    }

    [Fact]
    public void Create_Should_AttachUserNavigation_When_UserProvided()
    {
        var user = User.Create("a@b.com", "Name");

        var ur = UserRole.Create(10, user);

        ur.User.Should().Be(user);
        ur.RoleId.Should().Be(10);
    }
}
