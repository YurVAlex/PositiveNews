using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Security;

/// <summary>
/// Issues and inspects JWT access tokens for authenticated users.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a signed access token embedding the user identity and roles.
    /// </summary>
    /// <param name="user">The authenticated user entity.</param>
    /// <param name="roles">Role names granted to the user.</param>
    /// <returns>The encoded JWT string.</returns>
    string CreateAccessToken(User user, IReadOnlyCollection<string> roles);

    /// <summary>
    /// Returns the UTC expiry instant for access tokens produced by this service.
    /// </summary>
    /// <returns>The expiration time in UTC.</returns>
    DateTime GetAccessTokenExpiryUtc();
}
