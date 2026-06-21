namespace PositiveNews.Web.Api.Models;

public sealed class UserAdminDetailResponse
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool EmailConfirmed { get; init; }
    public int FailedLoginCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public long? ModeratedBy { get; init; }
}