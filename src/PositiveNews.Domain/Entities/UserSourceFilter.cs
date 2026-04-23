namespace PositiveNews.Domain.Entities;

public class UserSourceFilter
{
    // For EF Core materialization
    private UserSourceFilter() { }

    public long UserId { get; private set; }
    public int SourceId { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;
    public Source Source { get; private set; } = null!;

    public static UserSourceFilter Create(long userId, int sourceId)
    {
        return new UserSourceFilter { UserId = userId, SourceId = sourceId };
    }
}
