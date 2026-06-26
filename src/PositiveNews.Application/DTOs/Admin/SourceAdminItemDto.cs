namespace PositiveNews.Application.DTOs.Admin;

/// <summary>
/// Admin-facing source row for management tables.
/// </summary>
public sealed class SourceAdminItemDto
{
    /// <summary>Source primary key.</summary>
    public int Id { get; init; }

    /// <summary>Source display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Editorial trust weight.</summary>
    public decimal TrustScore { get; init; }

    /// <summary>Whether the source is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Last moderator user id who changed the source.</summary>
    public long? ModeratedBy { get; init; }
}
