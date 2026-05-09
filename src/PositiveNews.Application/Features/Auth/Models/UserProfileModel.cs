namespace PositiveNews.Application.Features.Auth.Models;

public sealed class UserProfileModel
{
    public long Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
