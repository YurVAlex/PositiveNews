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
            Role.Create("Admin"),
            Role.Create("Moderator"),
            Role.Create("User")
        };

        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} roles.", roles.Count);
    }

    private static async Task SeedTopicsAsync(AppDbContext context, ILogger logger, IConfiguration? configuration = null)
    {
        if (await context.Topics.AnyAsync()) return;

        List<Topic> topics;

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
            Topic.Create("Default", "default", "Not categorized"),
            Topic.Create("Technology", "technology,robotics,big tech,artificial intelligence,ai,software,innovations,hardware,internet", "Tech innovations and digital trends"),
            Topic.Create("Health", "health,wellness,medicine", "Wellness, medicine, and health science"),
            Topic.Create("Science", "science,research,energy", "Scientific discoveries and research"),
            Topic.Create("Environment", "environment,climate,ecology", "Climate, ecology, and conservation"),
            Topic.Create("Space", "space,nasa,solar system", "Astronomy, space exploration, and NASA"),
            Topic.Create("Travel", "travel,lifestyle", "Destinations and adventure"),
            Topic.Create("Animals", "animals,wildlife,pets,dogs,cats,birds", "Wildlife and animal stories"),
            Topic.Create("Inspiring", "inspiring,inspirations", "Uplifting and motivational stories"),
            Topic.Create("Arts & Culture", "arts,culture,murals,painting,illustration,illustrations,photography,portraits,surreal,anime,paintings,architectural,architecture,craft,creative,sculpture,woodworking,artist,books,design", "Art, music, literature, and culture"),
            Topic.Create("Education", "education,learning,teaching", "Learning, teaching, and academic breakthroughs"),
            Topic.Create("Business", "business,economy,startups,corporate,enterprise", "Economy, startups, and corporate responsibility"),
            Topic.Create("Sports", "sports", "Sports news"),
            Topic.Create("Gaming", "gaming", "Gaming news"),
            Topic.Create("Software", "software,cybersecurity,open source,data science", "Software news"),
            Topic.Create("Hardware", "GPU,RTX,GeForce", "New hardware news"),
            Topic.Create("Internet", "internet,omniverse,cloud", "Internet news"),
            Topic.Create("Evergreen", "evergreen", "Content that never goes out of style"),
            Topic.Create("Lifestyle", "lifestyle,nutrition,homelife,humor,human", "Interests, opinions, behaviours, and behavioural orientations"),
            Topic.Create("Trending", "trending", "About what's trending"),
            Topic.Create("Design", "design,style,fashion", "About design, style and fashion"),
            Topic.Create("Politics", "politics", "Latest Political News"),
            Topic.Create("Social", "social,community", "Latest Social News"),
            Topic.Create("Nature", "nature,animals,flowers,earth", "About earth and nature"),
            Topic.Create("Family", "family,divorce,parenting", "About family relationships"),
            Topic.Create("Psychology", "psychology,relationships,loneliness,family", "Psychology, relationships and mental health")
        };
    }

    private static async Task SeedSourcesAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Sources.AnyAsync()) return;

        var sources = new List<Source>
        {
            Source.Create("NVIDIA Blog", "https://blogs.nvidia.com/", "https://blogs.nvidia.com/feed/",
                "Latest news from NVIDIA.", "/Logos/Nvidia_logo.png", 1.0m, "en-US"),
            Source.Create("The Optimist Daily", "https://www.optimistdaily.com", "https://www.optimistdaily.com/feed/",
                "Making solutions the news.", "/Logos/Optimist_logo.png", 1.0m, "en"),
            Source.Create("NASA Breaking News", "https://www.nasa.gov", "https://www.nasa.gov/rss/dyn/breaking_news.rss",
                "Latest news from NASA.", "/Logos/NASA_logo.png", 1.0m, "en"),
            Source.Create("This Is Colossal News", "https://www.thisiscolossal.com", "https://www.thisiscolossal.com/feed",
                "Contemporary art and visual culture", "/Logos/Colossal_logo.png", 1.0m, "en"),
            Source.Create("Design You Trust", "https://designyoutrust.com", "https://designyoutrust.com/feed",
                "A daily dose of architecture and street art", "/Logos/Design_logo.png", 1.0m, "en"),
            Source.Create("Tiny Buddha", "https://tinybuddha.com", "https://tinybuddha.com/feed",
                "Psychology, self-development and happiness", "/Logos/Buddha_logo.png", 1.0m, "en")
        };

        context.Sources.AddRange(sources);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sources.", sources.Count);
    }

    private static async Task SeedAdminUserAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Users.AnyAsync()) return;

        var adminUser = User.Create("admin@positivenews.local", "System Administrator");
        adminUser.ConfirmEmail();

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
        context.UserRoles.Add(UserRole.Create(adminUser.Id, adminRole.Id));
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded admin user '{Email}' with Admin role.", adminUser.Email);
    }
}
