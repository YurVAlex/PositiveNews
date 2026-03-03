using Serilog;
using PositiveNews.Application;
using PositiveNews.Infrastructure;
using PositiveNews.Infrastructure.Persistence.Seeding;

namespace PositiveNews.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        // ---------------------------------------------------------------
        // 1. CONFIGURE SERILOG (Bootstrap logger for startup errors)
        // ---------------------------------------------------------------
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/positivenews-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting PositiveNews Web API...");

            var builder = WebApplication.CreateBuilder(args);

            // ---------------------------------------------------------------
            // 2. SERILOG — Replace default logging
            // ---------------------------------------------------------------
            builder.Host.UseSerilog((context, loggerConfig) =>
            {
                loggerConfig
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: "logs/positivenews-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
            });

            // ---------------------------------------------------------------
            // 3. REGISTER SERVICES (Clean Registration Pattern)
            // ---------------------------------------------------------------
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // ---------------------------------------------------------------
            // 4. SEED DATABASE (Create if not exists + populate reference data)
            // ---------------------------------------------------------------
            await DataSeeder.SeedAsync(app.Services);

            // ---------------------------------------------------------------
            // 5. MIDDLEWARE PIPELINE
            // ---------------------------------------------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            // ---------------------------------------------------------------
            // 6. RUN
            // ---------------------------------------------------------------
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