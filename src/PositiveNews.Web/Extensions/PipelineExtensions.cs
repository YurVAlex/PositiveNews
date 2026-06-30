using PositiveNews.Infrastructure.Extensions;
using Serilog;

namespace PositiveNews.Web.Extensions;

/// <summary>
/// HTTP middleware pipeline configuration for API and SPA hosting.
/// </summary>
public static class PipelineExtensions
{
    public const string TestingEnvironmentName = "Testing";

    /// <summary>
    /// Configures middleware, database initialization, and endpoint mapping.
    /// </summary>
    public static async Task<WebApplication> UsePositiveNewsPipelineAsync(this WebApplication app)
    {
        if (!app.Environment.IsEnvironment(TestingEnvironmentName))
        {
            await app.InitializeDatabase();
        }

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerDocumentation();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseSerilogRequestLogging();

        // Skip in Development so Vite (e.g. :5173) can proxy to HTTP without redirecting the browser to HTTPS (CORS).
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseSpaStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapFallbackToFile("index.html");

        return app;
    }

    private static IApplicationBuilder UseSpaStaticFiles(this IApplicationBuilder app)
    {
        var defaultFiles = new DefaultFilesOptions();
        defaultFiles.DefaultFileNames.Clear();
        defaultFiles.DefaultFileNames.Add("index.html");

        return app.UseDefaultFiles(defaultFiles).UseStaticFiles();
    }
}
