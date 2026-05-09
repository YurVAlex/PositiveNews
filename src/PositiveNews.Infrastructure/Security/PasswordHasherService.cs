using Microsoft.AspNetCore.Identity;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Security;

internal sealed class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
        => _passwordHasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string passwordHash, string providedPassword)
        => _passwordHasher.VerifyHashedPassword(user, passwordHash, providedPassword) != PasswordVerificationResult.Failed;
}
