using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using PositiveNews.Application;
using PositiveNews.Infrastructure;
using PositiveNews.Infrastructure.Extensions;
using PositiveNews.Infrastructure.Security;
using PositiveNews.Web.Api.ExceptionHandling;

namespace PositiveNews.Web;

/// <summary>
/// Host entry point for the PositiveNews web application (REST API and SPA hosting).
/// </summary>
public partial class Program
{
    /// <summary>
    /// Configures logging, services, authentication, middleware, and runs the web host.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    /// <returns>A task that completes when the host shuts down.</returns>
    /// <remarks>
    /// Initializes Serilog, registers application and infrastructure services, applies JWT authentication,
    /// serves OpenAPI in development, and maps controllers plus static SPA fallback.
    /// </remarks>
    /// <remarks>Synchronous entry point enables WebApplicationFactory integration tests to resolve the host.</remarks>
    public static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    private static async Task MainAsync(string[] args)
    {
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
            Log.Information("Starting PositiveNews Web (API + SPA)...");

            var builder = WebApplication.CreateBuilder(args);

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

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "PositiveNews API",
                        Version = "v1",
                        Description = "REST API for the PositiveNews application."
                    });

                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description =
                            "JWT Bearer token. Click Authorize, enter your token (without 'Bearer '), then call protected endpoints."
                    });

                // Microsoft.OpenApi 2.x: security requirement must reference the host document Swashbuckle is generating.
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document, string.Empty)] = []
                });
            });

            var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                ?? throw new InvalidOperationException("JWT configuration section is missing.");
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = signingKey,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (!app.Environment.IsEnvironment("Testing"))
            {
                await app.InitializeDatabase();
            }

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PositiveNews API v1");
                    options.DocumentTitle = "PositiveNews API";
                });
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();

            var defaultFiles = new DefaultFilesOptions();
            defaultFiles.DefaultFileNames.Clear();
            defaultFiles.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFiles);
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapFallbackToFile("index.html");

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
