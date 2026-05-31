using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Write access to refresh tokens.
/// </summary>
public interface IRefreshTokenWriteRepository
{
    /// <summary>
    /// Adds a new refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to add.</param>
    void Add(RefreshToken refreshToken);

    /// <summary>
    /// Updates an existing refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to update.</param>
    void Update(RefreshToken refreshToken);

    /// <summary>
    /// Removes a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to remove.</param>
    void Remove(RefreshToken refreshToken);
}
