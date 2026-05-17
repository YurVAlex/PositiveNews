using Microsoft.AspNetCore.Identity;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Security;

/// <summary>
/// ASP.NET Core Identity-compatible password hashing for <see cref="User"/> credentials.
/// </summary>
internal sealed class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    /// <inheritdoc />
    public string HashPassword(User user, string password)
        => _passwordHasher.HashPassword(user, password);

    /// <inheritdoc />
    public bool VerifyPassword(User user, string passwordHash, string providedPassword)
        => _passwordHasher.VerifyHashedPassword(user, passwordHash, providedPassword) != PasswordVerificationResult.Failed;
}
