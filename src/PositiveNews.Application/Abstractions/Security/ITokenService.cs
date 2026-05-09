using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Security;

public interface ITokenService
{
    string CreateAccessToken(User user, IReadOnlyCollection<string> roles);
    DateTime GetAccessTokenExpiryUtc();
}
