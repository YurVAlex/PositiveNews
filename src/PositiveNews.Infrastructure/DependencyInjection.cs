using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PositiveNews.Application.Interfaces;
using PositiveNews.Infrastructure.BackgroundJobs;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure;

/// <summary>
/// Clean Registration Pattern for the Infrastructure layer.
/// Call this single method from WebApi's Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core with SQL Server
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }));

        // HttpClient for RSS feed fetching.
        // Configured with a polite User-Agent and a reasonable timeout.
        services.AddHttpClient("RssFeedClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "PositiveNews/1.0 (+https://github.com/positivenews; Academic Project)");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/xml, text/xml");
        });

        // Application services
        services.AddScoped<IRssFeedReader, RssFeedReader>();
        services.AddScoped<IIngestionService, IngestionService>();

        // Background services
        services.AddHostedService<IngestionBackgroundService>();

        return services;
    }
}