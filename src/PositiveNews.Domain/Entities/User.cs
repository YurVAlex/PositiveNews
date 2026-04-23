using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

public class User
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<UserSourceFilter> _sourceFilters = [];
    private readonly List<UserTopicFilter> _topicFilters = [];
    private readonly List<Comment> _comments = [];

    // For EF Core materialization
    private User() { }

    public long Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginCount { get; private set; }
    public string? AvatarPictureUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public long? ModeratedBy { get; private set; }

    // Navigation
    public User? Moderator { get; private set; }
    public UserFeedPreference? FeedPreference { get; private set; }
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    public IReadOnlyCollection<UserSourceFilter> SourceFilters => _sourceFilters.AsReadOnly();
    public IReadOnlyCollection<UserTopicFilter> TopicFilters => _topicFilters.AsReadOnly();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    public static User Create(string email, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidUserStateException("User email cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidUserStateException("User name cannot be empty.");

        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            EmailConfirmed = false,
            FailedLoginCount = 0,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new InvalidUserStateException("Email cannot be empty.");
        Email = newEmail.Trim().ToLowerInvariant();
        EmailConfirmed = false;
    }

    public void SetPasswordHash(string? hash)
    {
        PasswordHash = hash;
    }

    public void SetAvatarUrl(string? url)
    {
        AvatarPictureUrl = url?.Trim();
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginCount = 0;
    }

    public void RecordFailedLogin()
    {
        FailedLoginCount++;
    }

    public void Deactivate(long moderatorId)
    {
        if (!IsActive)
            throw new InvalidUserStateException("User is already inactive.");
        IsActive = false;
        ModeratedBy = moderatorId;
    }
}
