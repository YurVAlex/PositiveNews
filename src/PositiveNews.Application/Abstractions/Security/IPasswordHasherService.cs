using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Security;

public interface IPasswordHasherService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string passwordHash, string providedPassword);
}
