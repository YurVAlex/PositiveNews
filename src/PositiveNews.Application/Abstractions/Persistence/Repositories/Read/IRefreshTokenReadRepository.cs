using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

/// <summary>
/// Read-only access to refresh tokens.
/// </summary>
public interface IRefreshTokenReadRepository
{
    /// <summary>
    /// Finds a refresh token by its token string.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The refresh token with user, or null if not found.</returns>
    Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a valid (not expired and not revoked) refresh token by its token string.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The valid refresh token with user, or null if not found or invalid.</returns>
    Task<RefreshToken?> FindValidByTokenAsync(string token, CancellationToken cancellationToken = default);
}
