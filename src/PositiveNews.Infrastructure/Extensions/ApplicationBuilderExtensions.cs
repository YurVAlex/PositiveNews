using Microsoft.AspNetCore.Builder;
using PositiveNews.Infrastructure.Persistence.Seeding;

namespace PositiveNews.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task<IApplicationBuilder> InitializeDatabase(this IApplicationBuilder app)
    {
        await DataSeeder.SeedAsync(app.ApplicationServices);
        return app;
    }
}
