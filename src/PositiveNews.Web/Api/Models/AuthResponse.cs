namespace PositiveNews.Web.Api.Models;

/// <summary>
/// JWT access token bundle returned after successful authentication.
/// </summary>
public sealed class AuthResponse
{
    /// <summary>
    /// Gets the bearer token string for API authorization.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets the UTC expiration instant for the access token.
    /// </summary>
    public DateTime ExpiresAtUtc { get; init; }

    /// <summary>
    /// Gets the refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets the authenticated user's profile snapshot.
    /// </summary>
    public UserProfileResponse User { get; init; } = new();
}
