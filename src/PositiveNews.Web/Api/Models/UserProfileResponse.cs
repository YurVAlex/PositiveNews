namespace PositiveNews.Web.Api.Models;

public sealed class UserProfileResponse
{
    public long Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
