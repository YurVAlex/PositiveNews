namespace PositiveNews.Application.DTOs.Admin;

/// <summary>
/// Complaint row shown in admin comment detail.
/// </summary>
public sealed class CommentComplaintAdminItemDto
{
    /// <summary>Complaint primary key.</summary>
    public long Id { get; init; }

    /// <summary>Complainant user id.</summary>
    public long UserId { get; init; }

    /// <summary>Complainant display name.</summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>Complaint reason text.</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>UTC time the complaint was filed.</summary>
    public DateTime CreatedAt { get; init; }
}
