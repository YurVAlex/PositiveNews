using FluentAssertions;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Tests.Api.Models;

public class UserProfileResponseTests
{
    [Fact]
    public void UserProfileResponse_Should_ExposeRolesList_When_Constructed()
    {
        var sut = new UserProfileResponse
        {
            Id = 5,
            Email = "a@b.com",
            Name = "X",
            Roles = ["User", "Admin"]
        };

        sut.Roles.Should().HaveCount(2);
        sut.Roles.Should().BeAssignableTo<IReadOnlyList<string>>();
    }
}
