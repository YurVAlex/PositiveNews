namespace PositiveNews.Application.DTOs.Auth;

/// <summary>
/// Non-sensitive user attributes exposed to clients after authentication.
/// </summary>
public sealed class UserProfileModel
{
    /// <summary>User primary key.</summary>
    public long Id { get; init; }

    /// <summary>Normalized email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Assigned role names.</summary>
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
