namespace PositiveNews.Web.Api.Models;

public sealed class CommentAdminDetailResponse
{
    public long Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public long? ModeratedBy { get; init; }
    public long ArticleId { get; init; }
    public IReadOnlyList<CommentComplaintAdminItemResponse> Complaints { get; init; } = [];
}
