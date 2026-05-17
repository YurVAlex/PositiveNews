namespace PositiveNews.Domain.Entities;

/// <summary>
/// Indicates that a user filters or follows a specific <see cref="Topic"/>.
/// </summary>
public class UserTopicFilter
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private UserTopicFilter() { }

    /// <summary>User owning this filter row.</summary>
    public long UserId { get; private set; }

    /// <summary>Selected topic.</summary>
    public int TopicId { get; private set; }

    /// <summary>Navigation to the user.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Navigation to the topic.</summary>
    public Topic Topic { get; private set; } = null!;

    /// <summary>
    /// Creates a user/topic pair for persistence.
    /// </summary>
    public static UserTopicFilter Create(long userId, int topicId)
    {
        return new UserTopicFilter { UserId = userId, TopicId = topicId };
    }
}
