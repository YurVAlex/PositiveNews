using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Seeding;

/// <summary>
/// Applies migrations (creates DB if missing) and seeds reference data.
/// Designed to be called once at application startup.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        logger.LogInformation("Applying database migrations (will create DB if it does not exist)...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        await SeedRolesAsync(context, logger);
        await SeedTopicsAsync(context, logger);
        await SeedSourcesAsync(context, logger);
        await SeedAdminUserAsync(context, logger);
    }

    private static async Task SeedRolesAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Roles.AnyAsync()) return;

        var roles = new List<Role>
        {
            new() { Name = "Admin" },
            new() { Name = "Moderator" },
            new() { Name = "User" }
        };

        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} roles.", roles.Count);
    }

    private static async Task SeedTopicsAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Topics.AnyAsync()) return;

        var topics = new List<Topic>
        {
            new() { Name = "Technology",       Slug = "technology",       Description = "Tech innovations and digital trends" },
            new() { Name = "Health",           Slug = "health",           Description = "Wellness, medicine, and health science" },
            new() { Name = "Science",          Slug = "science",          Description = "Scientific discoveries and research" },
            new() { Name = "Environment",      Slug = "environment",      Description = "Climate, ecology, and conservation" },
            new() { Name = "Space",            Slug = "space",            Description = "Astronomy, space exploration, and NASA" },
            new() { Name = "Travel",           Slug = "travel",           Description = "Destinations, culture, and adventure" },
            new() { Name = "Animals",          Slug = "animals",          Description = "Wildlife and animal stories" },
            new() { Name = "Inspiring",        Slug = "inspiring",        Description = "Uplifting and motivational stories" },
            new() { Name = "Arts & Culture",   Slug = "arts-culture",     Description = "Art, music, literature, and culture" },
            new() { Name = "Education",        Slug = "education",        Description = "Learning, teaching, and academic breakthroughs" },
            new() { Name = "Business",         Slug = "business",         Description = "Economy, startups, and corporate responsibility" },
            new() { Name = "Sports",           Slug = "sports",           Description = "Athletic achievements and sports news" },
            new() { Name = "General",          Slug = "general",          Description = "Uncategorized positive news" }
        };

        context.Topics.AddRange(topics);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} topics.", topics.Count);
    }

    private static async Task SeedSourcesAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Sources.AnyAsync()) return;

        var sources = new List<Source>
        {
            new()
            {
                Name = "Good News Network",
                BaseUrl = "https://www.goodnewsnetwork.org",
                FeedUrl = "https://www.goodnewsnetwork.org/feed/",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Since 1997, the premier source of positive news."
            },
            new()
            {
                Name = "The Optimist Daily",
                BaseUrl = "https://www.optimistdaily.com",
                FeedUrl = "https://www.optimistdaily.com/feed/",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Making solutions the news."
            },
            new()
            {
                Name = "NASA Breaking News",
                BaseUrl = "https://www.nasa.gov",
                FeedUrl = "https://www.nasa.gov/rss/dyn/breaking_news.rss",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Latest news from NASA."
            },
            new()
            {
                Name = "Space.com",
                BaseUrl = "https://www.space.com",
                FeedUrl = "https://www.space.com/feeds/all",
                TrustScore = 0.95m,
                DefaultLanguageCode = "en",
                Description = "Space exploration and astronomy news."
            },
            new()
            {
                Name = "National Geographic",
                BaseUrl = "https://www.nationalgeographic.com",
                FeedUrl = "https://www.nationalgeographic.com/foundation/news.rss",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Exploring and protecting our planet."
            },
            new()
            {
                Name = "Harvard Health",
                BaseUrl = "https://www.health.harvard.edu",
                FeedUrl = "https://www.health.harvard.edu/blog/feed",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Trusted health information from Harvard Medical School."
            },
            new()
            {
                Name = "Lonely Planet",
                BaseUrl = "https://www.lonelyplanet.com",
                FeedUrl = "https://www.lonelyplanet.com/news/feed",
                TrustScore = 0.9m,
                DefaultLanguageCode = "en",
                Description = "Travel guides and inspiration."
            }
        };

        context.Sources.AddRange(sources);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sources.", sources.Count);
    }

    private static async Task SeedAdminUserAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Users.AnyAsync()) return;

        // Seed a system/admin user to own future moderation actions.
        // In production, the password hash would come from a proper Identity hasher.
        var adminUser = new User
        {
            Email = "admin@positivenews.local",
            EmailConfirmed = true,
            Name = "System Administrator",
            PasswordHash = null, // Will be set via proper Auth flow later.
            IsActive = true
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
        context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded admin user '{Email}' with Admin role.", adminUser.Email);
    }
}