namespace PositiveNews.Domain.Entities;

/// <summary>
/// Indicates that a user limits or highlights a specific <see cref="Source"/> in their feed experience.
/// </summary>
public class UserSourceFilter
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private UserSourceFilter() { }

    /// <summary>User owning this filter row.</summary>
    public long UserId { get; private set; }

    /// <summary>Selected source.</summary>
    public int SourceId { get; private set; }

    /// <summary>Navigation to the user.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Navigation to the source.</summary>
    public Source Source { get; private set; } = null!;

    /// <summary>
    /// Creates a user/source pair for persistence.
    /// </summary>
    public static UserSourceFilter Create(long userId, int sourceId)
    {
        return new UserSourceFilter { UserId = userId, SourceId = sourceId };
    }
}
