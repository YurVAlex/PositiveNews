using PositiveNews.Web.Extensions;
using Serilog;

namespace PositiveNews.Web;

/// <summary>
/// Host entry point for the PositiveNews web application (REST API and SPA hosting).
/// </summary>
public class Program
{
    /// <summary>
    /// Configures logging, services, authentication, middleware, and runs the web host.
    /// </summary>
    public static async Task Main(string[] args)
    {
        Log.Logger = SerilogExtensions.CreateBootstrapLogger();

        try
        {
            Log.Information("Starting PositiveNews Web (API + SPA)...");

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UsePositiveNewsSerilog();
            builder.Services.AddWebServices(builder.Configuration);

            var app = builder.Build();
            await app.UsePositiveNewsPipelineAsync();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly.");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
