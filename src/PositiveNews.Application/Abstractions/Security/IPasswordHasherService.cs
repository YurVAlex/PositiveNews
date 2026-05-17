using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Security;

/// <summary>
/// Hashes and verifies user passwords using the application's configured algorithm.
/// </summary>
public interface IPasswordHasherService
{
    /// <summary>
    /// Produces a salted hash suitable for storing on <see cref="User.PasswordHash"/>.
    /// </summary>
    /// <param name="user">User context required by the underlying hasher implementation.</param>
    /// <param name="password">Plain-text password supplied by the user.</param>
    /// <returns>The persisted password hash.</returns>
    string HashPassword(User user, string password);

    /// <summary>
    /// Verifies a candidate password against a stored hash.
    /// </summary>
    /// <param name="user">User context required by the underlying hasher implementation.</param>
    /// <param name="passwordHash">Previously stored hash.</param>
    /// <param name="providedPassword">Candidate password to verify.</param>
    /// <returns><see langword="true"/> when the password matches the hash.</returns>
    bool VerifyPassword(User user, string passwordHash, string providedPassword);
}
