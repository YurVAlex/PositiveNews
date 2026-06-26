using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using PositiveNews.Infrastructure.Security;
using PositiveNews.Infrastructure.Tests.TestHelpers;

namespace PositiveNews.Infrastructure.Tests.Security;

public class JwtTokenServiceTests
{
    private static JwtOptions ValidOptions() => new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        SecretKey = new string('k', 64),
        AccessTokenMinutes = 60
    };

    [Fact]
    public void CreateAccessToken_Should_ReturnNonEmptyJwt_When_OptionsValid()
    {
        var sut = new JwtTokenService(Options.Create(ValidOptions()));
        var user = EntityBuilders.CreateUser();

        var token = sut.CreateAccessToken(user, ["User"]);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CreateAccessToken_Should_IncludeRoleClaims_When_RolesProvided()
    {
        var sut = new JwtTokenService(Options.Create(ValidOptions()));
        var user = EntityBuilders.CreateUser();

        var jwt = sut.CreateAccessToken(user, ["Admin", "Editor"]);

        var principal = ReadJwt(jwt);
        principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .Should().BeEquivalentTo(["Admin", "Editor"]);
    }

    [Fact]
    public void CreateAccessToken_Should_IncludeStandardClaims_When_UserHasProfile()
    {
        var sut = new JwtTokenService(Options.Create(ValidOptions()));
        var user = EntityBuilders.CreateUser("u@test.com", "User Name");

        var jwt = sut.CreateAccessToken(user, []);

        var principal = ReadJwt(jwt);
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be("u@test.com");
        principal.FindFirst(ClaimTypes.Name)?.Value.Should().Be("User Name");
        principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GetAccessTokenExpiryUtc_Should_BeWithinExpectedWindow_When_AccessTokenMinutesSet()
    {
        var o = ValidOptions();
        var options = new JwtOptions
        {
            Issuer = o.Issuer,
            Audience = o.Audience,
            SecretKey = o.SecretKey,
            AccessTokenMinutes = 30
        };
        var sut = new JwtTokenService(Options.Create(options));
        var before = DateTime.UtcNow;

        var expiry = sut.GetAccessTokenExpiryUtc();

        expiry.Should().BeOnOrAfter(before.AddMinutes(29));
        expiry.Should().BeOnOrBefore(before.AddMinutes(31));
    }

    [Fact]
    public void CreateAccessToken_Should_NotThrow_When_RolesEmpty()
    {
        var sut = new JwtTokenService(Options.Create(ValidOptions()));
        var user = EntityBuilders.CreateUser();

        var jwt = sut.CreateAccessToken(user, []);

        ReadJwt(jwt).Claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
    }

    [Fact]
    public void CreateRefreshTokenString_Should_ReturnNonEmptyString()
    {
        var sut = new JwtTokenService(Options.Create(ValidOptions()));

        var token = sut.CreateRefreshTokenString();

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CreateRefreshTokenString_Should_GenerateUniqueTokens()
    {
        var sut = new JwtTokenService(Options.Create(ValidOptions()));

        var token1 = sut.CreateRefreshTokenString();
        var token2 = sut.CreateRefreshTokenString();

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void CreateRefreshTokenString_Should_GenerateBase64EncodedString()
    {
        var sut = new JwtTokenService(Options.Create(ValidOptions()));

        var token = sut.CreateRefreshTokenString();

        var bytes = Convert.FromBase64String(token);
        bytes.Length.Should().Be(64);
    }

    [Fact]
    public void GetRefreshTokenExpiryUtc_Should_BeWithinExpectedWindow_When_RefreshTokenDaysSet()
    {
        var o = ValidOptions();
        var options = new JwtOptions
        {
            Issuer = o.Issuer,
            Audience = o.Audience,
            SecretKey = o.SecretKey,
            AccessTokenMinutes = 60,
            RefreshTokenDays = 7
        };
        var sut = new JwtTokenService(Options.Create(options));
        var before = DateTime.UtcNow;

        var expiry = sut.GetRefreshTokenExpiryUtc();

        expiry.Should().BeOnOrAfter(before.AddDays(6).AddHours(23));
        expiry.Should().BeOnOrBefore(before.AddDays(7).AddHours(1));
    }

    private static ClaimsPrincipal ReadJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return new ClaimsPrincipal(new ClaimsIdentity(jwt.Claims));
    }
}
