namespace PositiveNews.Web.Api.Models;

public sealed class CommentAdminItemResponse
{
    public long Id { get; init; }
    public long ArticleId { get; init; }
    public long UserId { get; init; }
    public int ComplaintCount { get; init; }
    public bool IsActive { get; init; }
    public long? ModeratedBy { get; init; }
}
