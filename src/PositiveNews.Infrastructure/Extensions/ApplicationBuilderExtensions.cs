using Microsoft.AspNetCore.Builder;
using PositiveNews.Infrastructure.Persistence.Seeding;

namespace PositiveNews.Infrastructure.Extensions;

/// <summary>
/// ASP.NET Core pipeline extensions for startup initialization.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations and seed data according to configuration.
    /// </summary>
    /// <param name="app">The application builder whose service provider is used for scope creation.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static async Task<IApplicationBuilder> InitializeDatabase(this IApplicationBuilder app)
    {
        await DataSeeder.SeedAsync(app.ApplicationServices);
        return app;
    }
}
