namespace PositiveNews.Domain.Entities;

public class UserSourceFilter
{
    public long UserId { get; set; }
    public int SourceId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Source Source { get; set; } = null!;
}