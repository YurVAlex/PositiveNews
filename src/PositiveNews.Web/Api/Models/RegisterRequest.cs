namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Request body for registering a new user account.
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// Gets the email address used as the login identity.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the public display name for the new account.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the plaintext password (transport should use HTTPS).
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
