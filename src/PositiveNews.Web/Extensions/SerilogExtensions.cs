using Serilog;

namespace PositiveNews.Web.Extensions;

/// <summary>
/// Serilog bootstrap and host logging configuration.
/// </summary>
public static class SerilogExtensions
{
    private const string LogFilePath = "logs/positivenews-.log";
    private const string LogOutputTemplate =
        "{Timestamp: MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Creates a bootstrap logger for startup and fatal error handling.
    /// </summary>
    public static Serilog.ILogger CreateBootstrapLogger() =>
        CreateBaseLoggerConfiguration()
            .CreateBootstrapLogger();

    /// <summary>
    /// Configures Serilog from application settings for the running host.
    /// </summary>
    public static IHostBuilder UsePositiveNewsSerilog(this IHostBuilder host) =>
        host.UseSerilog((context, config) =>
            config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: LogOutputTemplate)
                .WriteTo.File(
                    path: LogFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: LogOutputTemplate));

    private static LoggerConfiguration CreateBaseLoggerConfiguration() =>
        new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: LogOutputTemplate)
            .WriteTo.File(
                path: LogFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: LogOutputTemplate);
}
