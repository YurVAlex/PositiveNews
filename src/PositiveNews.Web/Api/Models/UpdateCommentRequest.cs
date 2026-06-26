namespace PositiveNews.Web.Api.Models;

public sealed class UpdateCommentRequest
{
    public bool IsActive { get; init; }
    public string? Reason { get; init; }
    public string? Note { get; init; }
}
