namespace PositiveNews.Domain.Entities;

public class UserTopicFilter
{
    // For EF Core materialization
    private UserTopicFilter() { }

    public long UserId { get; private set; }
    public int TopicId { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;
    public Topic Topic { get; private set; } = null!;

    public static UserTopicFilter Create(long userId, int topicId)
    {
        return new UserTopicFilter { UserId = userId, TopicId = topicId };
    }
}
