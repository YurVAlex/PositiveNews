using FluentAssertions;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Tests.Api.Models;

public class AuthResponseTests
{
    [Fact]
    public void AuthResponse_Should_HoldTokenAndExpiry_When_Constructed()
    {
        var expires = DateTime.UtcNow.AddHours(1);
        var sut = new AuthResponse
        {
            AccessToken = "jwt",
            ExpiresAtUtc = expires,
            User = new UserProfileResponse { Id = 1, Email = "e@test.com", Name = "N", Roles = ["User"] }
        };

        sut.AccessToken.Should().Be("jwt");
        sut.ExpiresAtUtc.Should().Be(expires);
        sut.User.Roles.Should().ContainSingle("User");
    }
}
