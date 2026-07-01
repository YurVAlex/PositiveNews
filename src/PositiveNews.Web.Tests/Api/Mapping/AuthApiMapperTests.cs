using FluentAssertions;
using PositiveNews.Application.DTOs.Auth;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Api.Mapping;

public class AuthApiMapperTests
{
    [Fact]
    public void ToAuthResponse_Should_MapTokenUserAndExpiry_When_ModelProvided()
    {
        var expires = DateTime.UtcNow.AddMinutes(30);
        var model = TestDataBuilders.AuthResult(expiresAtUtc: expires);

        var response = model.ToAuthResponse();

        response.AccessToken.Should().Be(model.AccessToken);
        response.ExpiresAtUtc.Should().Be(model.ExpiresAtUtc);
        response.User.Id.Should().Be(model.User.Id);
        response.User.Email.Should().Be(model.User.Email);
        response.User.Name.Should().Be(model.User.Name);
        response.User.Roles.Should().BeEquivalentTo(model.User.Roles);
    }

    [Fact]
    public void ToUserProfileResponse_Should_MapProfileFields_When_ModelProvided()
    {
        var model = TestDataBuilders.UserProfile(id: 3, roles: ["User", "Admin"]);

        var response = model.ToUserProfileResponse();

        response.Id.Should().Be(3);
        response.Email.Should().Be(model.Email);
        response.Name.Should().Be(model.Name);
        response.Roles.Should().BeEquivalentTo(["User", "Admin"]);
    }
}
