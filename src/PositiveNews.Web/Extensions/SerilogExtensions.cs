using Serilog;

namespace PositiveNews.Web.Extensions;

/// <summary>
/// Serilog bootstrap and host logging configuration.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Creates a minimal bootstrap logger for startup and fatal error handling.
    /// </summary>
    public static Serilog.ILogger CreateBootstrapLogger() =>
        new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

    /// <summary>
    /// Configures Serilog from application settings for the running host.
    /// </summary>
    public static IHostBuilder UsePositiveNewsSerilog(this IHostBuilder host) =>
        host.UseSerilog((context, config) =>
            config.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext());
}
