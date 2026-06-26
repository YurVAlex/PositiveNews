namespace PositiveNews.Application.DTOs.Admin;

/// <summary>
/// Admin-facing user row for the management table.
/// </summary>
public sealed class UserAdminItemDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool EmailConfirmed { get; init; }
    public int FailedLoginCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public long? ModeratedBy { get; init; }
}