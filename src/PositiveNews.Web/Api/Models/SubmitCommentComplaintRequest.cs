namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Request body for filing a complaint against a comment.
/// </summary>
public sealed class SubmitCommentComplaintRequest
{
    /// <summary>Complaint reason text.</summary>
    public string Reason { get; init; } = string.Empty;
}
