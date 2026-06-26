namespace PositiveNews.Web.Api.Models;

public sealed class CommentComplaintAdminItemResponse
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
