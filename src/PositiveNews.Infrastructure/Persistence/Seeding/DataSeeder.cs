using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Configuration;

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
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        logger.LogInformation("Applying database migrations (will create DB if it does not exist)...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        await SeedRolesAsync(context, logger);
        await SeedTopicsAsync(context, logger, configuration);
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

    private static async Task SeedTopicsAsync(AppDbContext context, ILogger logger, IConfiguration? configuration = null)
    {
        if (await context.Topics.AnyAsync()) return;

        List<Topic> topics;

        // ALWAYS try to load from configuration if provided
        if (configuration != null)
        {
            var topicConfig = configuration.GetSection("Topics").Get<TopicConfiguration>();
            if (topicConfig?.Topics != null && topicConfig.Topics.Any())
            {
                topics = topicConfig.Topics.Select(t => t.ToEntity()).ToList();
                logger.LogInformation("Loading topics from configuration.");
            }
            else
            {
                logger.LogWarning("No topics found in configuration, using defaults.");
                topics = GetDefaultTopics(logger);
            }
        }
        else
        {
            logger.LogWarning("No configuration provided, using defaults.");
            topics = GetDefaultTopics(logger);
        }

        context.Topics.AddRange(topics);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} topics.", topics.Count);
    }

    private static List<Topic> GetDefaultTopics(ILogger logger)
    {
        logger.LogInformation("Loading topics from configuration.");

        return new List<Topic>
    {
        new() { Name = "Default", Slug = "default", Description = "Not categorized" },
        new() { Name = "Technology", Slug = "technology,robotics,big tech,artificial intelligence,ai,software,innovations,hardware,internet", Description = "Tech innovations and digital trends" },
        new() { Name = "Health", Slug = "health,wellness,medicine", Description = "Wellness, medicine, and health science" },
        new() { Name = "Science", Slug = "science,research,energy", Description = "Scientific discoveries and research" },
        new() { Name = "Environment", Slug = "environment,climate,ecology", Description = "Climate, ecology, and conservation" },
        new() { Name = "Space", Slug = "space,nasa,solar system", Description = "Astronomy, space exploration, and NASA" },
        new() { Name = "Travel", Slug = "travel,lifestyle", Description = "Destinations and adventure" },
        new() { Name = "Animals", Slug = "animals,wildlife,pets,dogs,cats,birds", Description = "Wildlife and animal stories" },
        new() { Name = "Inspiring", Slug = "inspiring,inspirations", Description = "Uplifting and motivational stories" },
        new() { Name = "Arts & Culture", Slug = "arts,culture,murals,painting,illustration,illustrations,photography,portraits,surreal,anime,paintings,architectural,architecture,craft,creative,sculpture,woodworking,artist,books,design", Description = "Art, music, literature, and culture" },
        new() { Name = "Education", Slug = "education,learning,teaching", Description = "Learning, teaching, and academic breakthroughs" },
        new() { Name = "Business", Slug = "business,economy,startups,corporate,enterprise", Description = "Economy, startups, and corporate responsibility" },
        new() { Name = "Sports", Slug = "sports", Description = "Sports news" },
        new() { Name = "Gaming", Slug = "gaming", Description = "Gaming news" },
        new() { Name = "Software", Slug = "software,cybersecurity,open source,data science", Description = "Software news" },
        new() { Name = "Hardware", Slug = "GPU,RTX,GeForce", Description = "New hardware news" },
        new() { Name = "Internet", Slug = "internet,omniverse,cloud", Description = "Internet news" },
        new() { Name = "Evergreen", Slug = "evergreen", Description = "Content that never goes out of style" },
        new() { Name = "Lifestyle", Slug = "lifestyle,nutrition,homelife,humor,human", Description = "Interests, opinions, behaviours, and behavioural orientations" },
        new() { Name = "Trending", Slug = "trending", Description = "About what's trending" },
        new() { Name = "Design", Slug = "design,style,fashion", Description = "About design, style and fashion" },
        new() { Name = "Politics", Slug = "politics", Description = "Latest Political News" },
        new() { Name = "Social", Slug = "social,community", Description = "Latest Social News" },
        new() { Name = "Nature", Slug = "nature,animals,flowers,earth", Description = "About earth and nature" },
        new() { Name = "Family", Slug = "family,divorce,parenting", Description = "About family relationships" },
        new() { Name = "Psychology", Slug = "psychology,relationships,loneliness,family", Description = "Psychology, relationships and mental health" }
    };
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
                LogoUrl = "/Logos/Nvidia_logo.png"
            },
            new()
            {
                Name = "The Optimist Daily",
                BaseUrl = "https://www.optimistdaily.com",
                FeedUrl = "https://www.optimistdaily.com/feed/",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Making solutions the news.",
                LogoUrl = "/Logos/Optimist_logo.png"
            },
            new()
            {
                Name = "NASA Breaking News",
                BaseUrl = "https://www.nasa.gov",
                FeedUrl = "https://www.nasa.gov/rss/dyn/breaking_news.rss",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Latest news from NASA.",
                LogoUrl = "/Logos/NASA_logo.png"
            },
            new()
            {
                Name = "This Is Colossal News",
                BaseUrl = "https://www.thisiscolossal.com",
                FeedUrl = "https://www.thisiscolossal.com/feed",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Contemporary art and visual culture",
                LogoUrl = "/Logos/Colossal_logo.png"
            },
            new()
            {
                Name = "Design You Trust",
                BaseUrl = "https://designyoutrust.com",
                FeedUrl = "https://designyoutrust.com/feed",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "A daily dose of architecture and street art",
                LogoUrl = "/Logos/Design_logo.png"
            },
            new()
            {
                Name = "Tiny Buddha",
                BaseUrl = "https://tinybuddha.com",
                FeedUrl = "https://tinybuddha.com/feed",
                TrustScore = 1.0m,
                DefaultLanguageCode = "en",
                Description = "Psychology, self-development and happiness",
                LogoUrl = "/Logos/Buddha_logo.png"
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