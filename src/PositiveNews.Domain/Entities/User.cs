using System.Xml.Linq;

namespace PositiveNews.Domain.Entities;

public class User
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginCount { get; set; }
    public string? AvatarPictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public long? ModeratedBy { get; set; }

    // Navigation
    public User? Moderator { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public UserFeedPreference? FeedPreference { get; set; }
    public ICollection<UserSourceFilter> SourceFilters { get; set; } = new List<UserSourceFilter>();
    public ICollection<UserTopicFilter> TopicFilters { get; set; } = new List<UserTopicFilter>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}