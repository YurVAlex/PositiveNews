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
            new() { Name = "Default",          Slug = "default",          Description = "Not categorized" },
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
                Name = "NVIDIA Blog",
                BaseUrl = "https://blogs.nvidia.com/",
                FeedUrl = "https://blogs.nvidia.com/feed/",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en-US",
                Description = "Latest news from NVIDIA.",
                LogoUrl = "https://upload.wikimedia.org/wikipedia/sco/thumb/2/21/Nvidia_logo.svg/250px-Nvidia_logo.svg.png"
            },
            new()
            {
                Name = "The Optimist Daily",
                BaseUrl = "https://www.optimistdaily.com",
                FeedUrl = "https://www.optimistdaily.com/feed/",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Making solutions the news.",
                LogoUrl = "https://www.optimistdaily.com/wp-content/themes/magazine-pro/images/logo.png"
            },
            new()
            {
                Name = "NASA Breaking News",
                BaseUrl = "https://www.nasa.gov",
                FeedUrl = "https://www.nasa.gov/rss/dyn/breaking_news.rss",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Latest news from NASA.",
                LogoUrl = "https://www.nasa.gov/wp-content/themes/nasa/assets/images/nasa-logo@2x.png"
            },
            new()
            {
                Name = "This Is Colossal News",
                BaseUrl = "https://www.thisiscolossal.com",
                FeedUrl = "https://www.thisiscolossal.com/feed",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Contemporary art and visual culture",
                LogoUrl = "https://www.thisiscolossal.com/wp-content/uploads/2024/08/icon-crow-150x150.png"
            },
            new()
            {
                Name = "Design You Trust",
                BaseUrl = "https://designyoutrust.com",
                FeedUrl = "https://designyoutrust.com/feed",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "A daily dose of architecture and street art",
                LogoUrl = "https://img.ws.mms.shopee.com.br/78107abce75961845158eb136bdc9290"
            },
            new()
            {
                Name = "Tiny Buddha",
                BaseUrl = "https://tinybuddha.com",
                FeedUrl = "https://tinybuddha.com/feed",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Psychology, self-development and happiness",
                LogoUrl = "https://cdn.tinybuddha.com/wp-content/themes/tinybuddha/images/logo.png"
            },
            new()
            {
                Name = "Raptitude",
                BaseUrl = "https://raptitude.com",
                FeedUrl = "https://www.raptitude.com/feed/",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Smart about the main thing",
                LogoUrl = "https://ideanomics.ru/wp-content/uploads/2021/11/raptitude-300x300.png"
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