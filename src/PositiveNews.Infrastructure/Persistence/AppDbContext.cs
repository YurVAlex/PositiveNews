using Microsoft.EntityFrameworkCore;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Identity
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserFeedPreference> UserFeedPreferences => Set<UserFeedPreference>();
    public DbSet<UserSourceFilter> UserSourceFilters => Set<UserSourceFilter>();
    public DbSet<UserTopicFilter> UserTopicFilters => Set<UserTopicFilter>();

    // Catalog
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<ArticleMetadata> ArticlesMetadata => Set<ArticleMetadata>();
    public DbSet<ArticleContent> ArticlesContent => Set<ArticleContent>();
    public DbSet<ArticleTopic> ArticleTopics => Set<ArticleTopic>();
    public DbSet<IngestionRun> IngestionRuns => Set<IngestionRun>();

    // Community
    public DbSet<Comment> Comments => Set<Comment>();

    // Admin
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applies all IEntityTypeConfiguration<T> from this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}