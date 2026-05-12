namespace PositiveNews.Infrastructure.Security;

/// <summary>
/// Options for signing and validating JWT access tokens (bound from configuration).
/// </summary>
public sealed class JwtOptions
{
    /// <summary>Configuration section name (<c>Jwt</c>).</summary>
    public const string SectionName = "Jwt";

    /// <summary>JWT <c>iss</c> claim and validation issuer.</summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>JWT <c>aud</c> claim and validation audience.</summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>Symmetric key material for HMAC-SHA256 signing.</summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>Lifetime of issued access tokens in minutes.</summary>
    public int AccessTokenMinutes { get; init; } = 120;
}
