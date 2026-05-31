using FluentAssertions;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Domain.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_Should_SetPropertiesCorrectly_When_ValidInputProvided()
    {
        var token = "test-token";
        var userId = 123L;
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var refreshToken = RefreshToken.Create(token, userId, expiresAt);

        refreshToken.Token.Should().Be(token);
        refreshToken.UserId.Should().Be(userId);
        refreshToken.ExpiresAtUtc.Should().Be(expiresAt);
        refreshToken.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public void IsValid_Should_ReturnTrue_When_TokenNotExpiredAndNotRevoked()
    {
        var refreshToken = RefreshToken.Create("token", 1, DateTime.UtcNow.AddDays(7));

        var isValid = refreshToken.IsValid();

        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_TokenRevoked()
    {
        var refreshToken = RefreshToken.Create("token", 1, DateTime.UtcNow.AddDays(7));
        refreshToken.Revoke();

        var isValid = refreshToken.IsValid();

        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_TokenExpired()
    {
        var refreshToken = RefreshToken.Create("token", 1, DateTime.UtcNow.AddDays(-1));

        var isValid = refreshToken.IsValid();

        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_TokenExpiredAndRevoked()
    {
        var refreshToken = RefreshToken.Create("token", 1, DateTime.UtcNow.AddDays(-1));
        refreshToken.Revoke();

        var isValid = refreshToken.IsValid();

        isValid.Should().BeFalse();
    }

    [Fact]
    public void Revoke_Should_SetIsRevokedToTrue_When_Called()
    {
        var refreshToken = RefreshToken.Create("token", 1, DateTime.UtcNow.AddDays(7));

        refreshToken.Revoke();

        refreshToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_Should_SetRevokedAtUtc_When_Called()
    {
        var refreshToken = RefreshToken.Create("token", 1, DateTime.UtcNow.AddDays(7));
        var before = DateTime.UtcNow;

        refreshToken.Revoke();

        refreshToken.RevokedAtUtc.Should().NotBeNull();
        refreshToken.RevokedAtUtc!.Value.Should().BeOnOrAfter(before);
        refreshToken.RevokedAtUtc!.Value.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Revoke_Should_NotChangeRevokedAtUtc_When_AlreadyRevoked()
    {
        var refreshToken = RefreshToken.Create("token", 1, DateTime.UtcNow.AddDays(7));
        refreshToken.Revoke();
        var firstRevokedAt = refreshToken.RevokedAtUtc;

        System.Threading.Thread.Sleep(10);
        refreshToken.Revoke();

        refreshToken.RevokedAtUtc.Should().Be(firstRevokedAt);
    }
}
