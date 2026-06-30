namespace PositiveNews.Domain.Constants;

/// <summary>
/// Canonical role names used for authorization and seed data.
/// </summary>
public static class RoleNames
{
    /// <summary>Full administrative access.</summary>
    public const string Admin = "Admin";

    /// <summary>Content moderation privileges.</summary>
    public const string Moderator = "Moderator";

    /// <summary>Standard registered user.</summary>
    public const string User = "User";
}
