using Microsoft.EntityFrameworkCore;
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
        if (await context.Sources.AnyAsync()) return;

        if (seedConfig.Sources.Count == 0)
        {
            logger.LogWarning("No sources found in SeedData configuration. Skipping source seeding.");
            return;
        }

        var sources = seedConfig.Sources.Select(s => s.ToEntity()).ToList();
        context.Sources.AddRange(sources);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sources from configuration.", sources.Count);
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
