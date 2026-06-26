using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Configuration;

namespace PositiveNews.Infrastructure.Persistence.Seeding;

/// <summary>
/// Applies migrations (creates DB if missing) and seeds reference data
/// exclusively from the "SeedData" configuration section.
/// Designed to be called once at application startup.
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Applies EF migrations and seeds roles, topics, sources, and the default admin user when configured.
    /// </summary>
    /// <param name="serviceProvider">Root DI provider used to resolve <see cref="AppDbContext"/> and configuration.</param>
    /// <returns>A task representing the asynchronous seed operation.</returns>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        logger.LogInformation("Applying database migrations (will create DB if it does not exist)...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        var seedConfig = configuration.GetSection("SeedData").Get<SeedDataConfiguration>();
        if (seedConfig is null)
        {
            logger.LogWarning("No 'SeedData' section found in configuration. Skipping all seeding.");
            return;
        }

        await SeedRolesAsync(context, logger, seedConfig);
        await SeedTopicsAsync(context, logger, seedConfig);
        await SeedSourcesAsync(context, logger, seedConfig);
        await SeedAdminUserAsync(context, logger);
    }

    private static async Task SeedRolesAsync(AppDbContext context, ILogger logger, SeedDataConfiguration seedConfig)
    {
        if (await context.Roles.AnyAsync()) return;

        if (seedConfig.Roles.Count == 0)
        {
            logger.LogWarning("No roles found in SeedData configuration. Skipping role seeding.");
            return;
        }

        var roles = seedConfig.Roles.Select(r => r.ToEntity()).ToList();
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} roles from configuration.", roles.Count);
    }

    private static async Task SeedTopicsAsync(AppDbContext context, ILogger logger, SeedDataConfiguration seedConfig)
    {
        if (await context.Topics.AnyAsync()) return;

        if (seedConfig.Topics.Count == 0)
        {
            logger.LogWarning("No topics found in SeedData configuration. Skipping topic seeding.");
            return;
        }

        var topics = seedConfig.Topics.Select(t => t.ToEntity()).ToList();
        context.Topics.AddRange(topics);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} topics from configuration.", topics.Count);
    }

    private static async Task SeedSourcesAsync(AppDbContext context, ILogger logger, SeedDataConfiguration seedConfig)
    {
        if (seedConfig.Sources.Count == 0)
        {
            logger.LogWarning("No sources found in SeedData configuration. Skipping source seeding.");
            return;
        }

        var existingSources = await context.Sources
            .AsNoTracking()
            .Select(s => new { s.Name, s.FeedUrl })
            .ToListAsync();

        var existingFeedUrls = new HashSet<string>(
            existingSources
                .Where(s => !string.IsNullOrWhiteSpace(s.FeedUrl))
                .Select(s => s.FeedUrl!.Trim()),
            StringComparer.OrdinalIgnoreCase);

        var existingNames = new HashSet<string>(
            existingSources
                .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                .Select(s => s.Name.Trim()),
            StringComparer.OrdinalIgnoreCase);

        var seenFeedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var sourcesToSeed = new List<Source>();

        foreach (var entry in seedConfig.Sources)
        {
            if (SourceExists(entry, existingFeedUrls, existingNames))
            {
                continue;
            }

            var feedUrl = entry.FeedUrl?.Trim();
            if (!string.IsNullOrWhiteSpace(feedUrl))
            {
                if (!seenFeedUrls.Add(feedUrl))
                    continue;
            }
            else
            {
                var name = entry.Name.Trim();
                if (!seenNames.Add(name))
                    continue;
            }

            sourcesToSeed.Add(entry.ToEntity());
        }

        if (sourcesToSeed.Count == 0)
        {
            logger.LogInformation("No new sources to seed from configuration.");
            return;
        }

        context.Sources.AddRange(sourcesToSeed);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} new source(s) from configuration.", sourcesToSeed.Count);
    }

    private static bool SourceExists(SourceEntry entry, HashSet<string> existingFeedUrls, HashSet<string> existingNames)
    {
        var feedUrl = entry.FeedUrl?.Trim();
        if (!string.IsNullOrWhiteSpace(feedUrl) && existingFeedUrls.Contains(feedUrl))
            return true;

        var name = entry.Name.Trim();
        return existingNames.Contains(name);
    }

    private static async Task SeedAdminUserAsync(AppDbContext context, ILogger logger)
    {
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is null)
        {
            logger.LogWarning("Admin role is missing. Cannot seed admin user.");
            return;
        }

        var adminEmail = "admin@positivenews.local";
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (adminUser is null)
        {
            adminUser = User.Create(adminEmail, "First Administrator");
            adminUser.ConfirmEmail();
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        if (string.IsNullOrWhiteSpace(adminUser.PasswordHash))
        {
            adminUser.SetPasswordHash(new PasswordHasher<User>().HashPassword(adminUser, "Admin123!"));
            await context.SaveChangesAsync();
            logger.LogInformation("Set default password for admin user '{Email}'.", adminUser.Email);
        }

        var hasAdminRole = await context.UserRoles
            .AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
        if (!hasAdminRole)
        {
            context.UserRoles.Add(UserRole.Create(adminUser.Id, adminRole.Id));
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Ensured admin user '{Email}' has Admin role.", adminUser.Email);
    }
}
