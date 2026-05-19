using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// Application user: credentials, profile, roles, optional feed filters, and authored comments.
/// </summary>
public class User
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<UserSourceFilter> _sourceFilters = [];
    private readonly List<UserTopicFilter> _topicFilters = [];
    private readonly List<Comment> _comments = [];

    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private User() { }

    /// <summary>Primary key.</summary>
    public long Id { get; private set; }

    /// <summary>Normalized email address (unique).</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Whether the email ownership was verified.</summary>
    public bool EmailConfirmed { get; private set; }

    /// <summary>Display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Password hash (never store plaintext).</summary>
    public string? PasswordHash { get; private set; }

    /// <summary>Last successful login instant, UTC.</summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>Consecutive failed login attempts (reset on success).</summary>
    public int FailedLoginCount { get; private set; }

    /// <summary>Optional avatar image URL.</summary>
    public string? AvatarPictureUrl { get; private set; }

    /// <summary>Account creation time, UTC.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>When false, the user cannot sign in.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Moderator who deactivated the account, if any.</summary>
    public long? ModeratedBy { get; private set; }

    /// <summary>Moderator navigation.</summary>
    public User? Moderator { get; private set; }

    /// <summary>Optional personalized feed settings.</summary>
    public UserFeedPreference? FeedPreference { get; private set; }

    /// <summary>Roles granted to this user.</summary>
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    /// <summary>Preferred sources for feed filtering.</summary>
    public IReadOnlyCollection<UserSourceFilter> SourceFilters => _sourceFilters.AsReadOnly();

    /// <summary>Preferred topics for feed filtering.</summary>
    public IReadOnlyCollection<UserTopicFilter> TopicFilters => _topicFilters.AsReadOnly();

    /// <summary>Comments authored by this user.</summary>
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    /// <summary>
    /// Creates a new active user with normalized email and trimmed display name.
    /// </summary>
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

    /// <summary>Marks the email as confirmed.</summary>
    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    /// <summary>
    /// Changes the email (normalized) and clears confirmation until re-verified.
    /// </summary>
    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new InvalidUserStateException("Email cannot be empty.");
        Email = newEmail.Trim().ToLowerInvariant();
        EmailConfirmed = false;
    }

    /// <summary>Stores the password hash produced by the application password hasher.</summary>
    public void SetPasswordHash(string? hash)
    {
        PasswordHash = hash;
    }

    /// <summary>Sets or clears the avatar URL (trimmed).</summary>
    public void SetAvatarUrl(string? url)
    {
        AvatarPictureUrl = url?.Trim();
    }

    /// <summary>
    /// Records a successful login: updates last login time and clears failed attempts.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginCount = 0;
    }

    /// <summary>Increments failed login attempts (e.g. after a bad password).</summary>
    public void RecordFailedLogin()
    {
        FailedLoginCount++;
    }

    private const string DeletedEmailSuffix = ".deleted";
    private const string DeletedUserDisplayName = "Deleted user";
    private const int MaxEmailLength = 300;

    /// <summary>
    /// Deactivates the account, anonymizes profile data, and records the acting moderator.
    /// </summary>
    public void Deactivate(long moderatorId)
    {
        if (!IsActive)
            throw new InvalidUserStateException("User is already inactive.");

        Email = ToDeletedEmail(Email);
        Name = DeletedUserDisplayName;
        IsActive = false;
        ModeratedBy = moderatorId;
    }

    private static string ToDeletedEmail(string email)
    {
        if (email.EndsWith(DeletedEmailSuffix, StringComparison.Ordinal))
        {
            return email;
        }

        var combined = email + DeletedEmailSuffix;
        if (combined.Length <= MaxEmailLength)
        {
            return combined;
        }

        return email[..(MaxEmailLength - DeletedEmailSuffix.Length)] + DeletedEmailSuffix;
    }
}
