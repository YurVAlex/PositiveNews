using Microsoft.EntityFrameworkCore;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for PositiveNews persistence (identity, catalog, community, admin).
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">Context options including provider and connection.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Roles available in the application.</summary>
    public DbSet<Role> Roles => Set<Role>();
    /// <summary>Registered user accounts.</summary>
    public DbSet<User> Users => Set<User>();
    /// <summary>Many-to-many assignment of users to roles.</summary>
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    /// <summary>Per-user feed sorting and positivity preferences.</summary>
    public DbSet<UserFeedPreference> UserFeedPreferences => Set<UserFeedPreference>();
    /// <summary>Optional per-user inclusion filters for news sources.</summary>
    public DbSet<UserSourceFilter> UserSourceFilters => Set<UserSourceFilter>();
    /// <summary>Optional per-user inclusion filters for topics.</summary>
    public DbSet<UserTopicFilter> UserTopicFilters => Set<UserTopicFilter>();
    /// <summary>Refresh tokens for obtaining new access tokens.</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>Topic taxonomy used for categorization and filtering.</summary>
    public DbSet<Topic> Topics => Set<Topic>();
    /// <summary>News sources (RSS/API) and metadata.</summary>
    public DbSet<Source> Sources => Set<Source>();
    /// <summary>Article headline and feed metadata.</summary>
    public DbSet<ArticleMetadata> ArticlesMetadata => Set<ArticleMetadata>();
    /// <summary>Full HTML body and cleaned content for articles.</summary>
    public DbSet<ArticleContent> ArticlesContent => Set<ArticleContent>();
    /// <summary>Article-to-topic associations.</summary>
    public DbSet<ArticleTopic> ArticleTopics => Set<ArticleTopic>();
    /// <summary>Recorded ingest attempts per source.</summary>
    public DbSet<IngestionRun> IngestionRuns => Set<IngestionRun>();

    /// <summary>User comments on articles.</summary>
    public DbSet<Comment> Comments => Set<Comment>();

    /// <summary>User complaints about comments.</summary>
    public DbSet<Complaint> Complains => Set<Complaint>();

    /// <summary>Administrative audit trail.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
