namespace PositiveNews.Application.Features.Auth.Models;

public sealed class AuthResultModel
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public UserProfileModel User { get; init; } = new();
}
