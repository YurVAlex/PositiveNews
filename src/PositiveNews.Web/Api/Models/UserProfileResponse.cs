namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Authenticated user profile returned by the API.
/// </summary>
public sealed class UserProfileResponse
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the role names granted to the user.
    /// </summary>
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
