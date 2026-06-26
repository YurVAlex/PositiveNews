namespace PositiveNews.Domain.Entities;

/// <summary>
/// Refresh token for obtaining new access tokens without re-authentication.
/// </summary>
public class RefreshToken
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private RefreshToken() { }

    /// <summary>Primary key.</summary>
    public long Id { get; private set; }

    /// <summary>The unique token string.</summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>User who owns this refresh token.</summary>
    public long UserId { get; private set; }

    /// <summary>Navigation to the user.</summary>
    public User User { get; private set; } = null!;

    /// <summary>When the token expires (UTC).</summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>When the token was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Whether the token has been revoked.</summary>
    public bool IsRevoked { get; private set; }

    /// <summary>When the token was revoked (UTC).</summary>
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Creates a new refresh token for a user.
    /// </summary>
    /// <param name="token">The unique token string.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="expiresAtUtc">When the token expires.</param>
    /// <returns>A new RefreshToken instance.</returns>
    public static RefreshToken Create(string token, long userId, DateTime expiresAtUtc)
    {
        return new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow,
            IsRevoked = false
        };
    }

    /// <summary>
    /// Checks if the token is currently valid (not expired and not revoked).
    /// </summary>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return !IsRevoked && DateTime.UtcNow < ExpiresAtUtc;
    }

    /// <summary>
    /// Revokes the token, making it invalid for future use.
    /// </summary>
    public void Revoke()
    {
        if (!IsRevoked)
        {
            RevokedAtUtc = DateTime.UtcNow;
        }
        IsRevoked = true;
    }
}
