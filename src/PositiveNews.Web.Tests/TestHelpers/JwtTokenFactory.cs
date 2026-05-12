using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PositiveNews.Web.Tests.TestHelpers;

/// <summary>
/// Issues JWT access tokens compatible with Web API JwtBearer configuration used in tests.
/// Values align with <c>appsettings.json</c> / merged Testing overrides.
/// </summary>
internal static class JwtTokenFactory
{
    /// <summary>Same issuer as configured Jwt:Issuer for local/dev runs.</summary>
    public const string DefaultIssuer = "PositiveNews.Web";

    /// <summary>Same audience as configured Jwt:Audience.</summary>
    public const string DefaultAudience = "PositiveNews.Client";

    /// <summary>HMAC key — must match appsettings Jwt:SecretKey used when validating tokens.</summary>
    public const string DefaultSecretKey = "PositiveNews_DevOnly_SuperSecretKey_ChangeInProduction_2026";

    public static string CreateAccessToken(
        string userId,
        IEnumerable<string>? roles = null,
        string issuer = DefaultIssuer,
        string audience = DefaultAudience,
        string secretKey = DefaultSecretKey,
        DateTime? expiresUtc = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (roles != null)
        {
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = expiresUtc ?? DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
