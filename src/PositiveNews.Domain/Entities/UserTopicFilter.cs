namespace PositiveNews.Domain.Entities;

public class UserTopicFilter
{
    public long UserId { get; set; }
    public int TopicId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
}