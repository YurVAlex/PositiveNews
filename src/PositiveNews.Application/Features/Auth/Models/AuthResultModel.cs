namespace PositiveNews.Application.Features.Auth.Models;

/// <summary>
/// Successful authentication payload returned by login and registration flows.
/// </summary>
public sealed class AuthResultModel
{
    /// <summary>Bearer JWT access token.</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>UTC instant when the access token expires.</summary>
    public DateTime ExpiresAtUtc { get; init; }

    /// <summary>Basic profile for the authenticated user.</summary>
    public UserProfileModel User { get; init; } = new();
}
