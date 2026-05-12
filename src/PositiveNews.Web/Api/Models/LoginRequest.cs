namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Request body for authenticating an existing user.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// Gets the account email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the account password.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
