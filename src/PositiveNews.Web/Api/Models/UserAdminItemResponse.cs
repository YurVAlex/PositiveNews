namespace PositiveNews.Web.Api.Models;

public sealed class UserAdminItemResponse
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool EmailConfirmed { get; init; }
    public int FailedLoginCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public long? ModeratedBy { get; init; }
}