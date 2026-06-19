using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PositiveNews.Application.Abstractions.Ingestion;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Application.Interfaces;
using PositiveNews.Application.Services.Ingestion;
using PositiveNews.Infrastructure.BackgroundJobs;
using PositiveNews.Infrastructure.Configuration;
using PositiveNews.Infrastructure.Ingestion;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Infrastructure.Persistence.Connection;
using PositiveNews.Infrastructure.Persistence.Repositories.Read;
using PositiveNews.Infrastructure.Persistence.Repositories.Write;
using PositiveNews.Infrastructure.Persistence.UnitOfWork;
using PositiveNews.Infrastructure.Security;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure;

/// <summary>
/// Clean Registration Pattern for the Infrastructure layer.
/// Call this single method from WebApi's Program.cs.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers EF Core, repositories, unit-of-work, security, ingestion services, HTTP clients, and the ingestion background job.
    /// </summary>
    /// <param name="services">The application's service collection.</param>
    /// <param name="configuration">Application configuration (connection strings, JWT, ingestion, etc.).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = ConnectionStringResolver.Resolve(configuration);

        services.AddDbContext<AppDbContext>(options =>
               options.UseSqlServer(connectionString, sqlOptions =>
                      {
                          sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                          sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                      }));

        services.AddSingleton<IIngestionCycleCoordinator, IngestionCycleCoordinator>();

        services.AddScoped<IArticleReadRepository, ArticleReadRepository>();
        services.AddScoped<ITopicReadRepository, TopicReadRepository>();
        services.AddScoped<IAuditLogReadRepository, AuditLogReadRepository>();
        services.AddScoped<ISourceReadRepository, SourceReadRepository>();
        services.AddScoped<IIngestionRunReadRepository, IngestionRunReadRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserFeedPreferencesReadRepository, UserFeedPreferencesReadRepository>();
        services.AddScoped<IRoleReadRepository, RoleReadRepository>();
        services.AddScoped<IRefreshTokenReadRepository, RefreshTokenReadRepository>();
        services.AddScoped<ICommentReadRepository, CommentReadRepository>();

        services.AddScoped<IArticleWriteRepository, ArticleWriteRepository>();
        services.AddScoped<IArticleTopicWriteRepository, ArticleTopicWriteRepository>();
        services.AddScoped<ITopicWriteRepository, TopicWriteRepository>();
        services.AddScoped<ISourceWriteRepository, SourceWriteRepository>();
        services.AddScoped<IAuditLogWriteRepository, AuditLogWriteRepository>();
        services.AddScoped<IIngestionRunRepository, IngestionRunRepository>();
        services.AddScoped<IUserWriteRepository, UserWriteRepository>();
        services.AddScoped<IUserFeedPreferencesWriteRepository, UserFeedPreferencesWriteRepository>();
        services.AddScoped<IUserRoleWriteRepository, UserRoleWriteRepository>();
        services.AddScoped<IRefreshTokenWriteRepository, RefreshTokenWriteRepository>();
        services.AddScoped<ICommentWriteRepository, CommentWriteRepository>();
        services.AddScoped<IComplaintWriteRepository, ComplaintWriteRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIngestionUnitOfWork, IngestionUnitOfWork>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();

        services.AddHttpClient("RssFeedClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
               "PositiveNews/1.0 (+https://github.com/positivenews; Academic Project)");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/xml, text/xml");
        });

        services.Configure<IngestionSettingsConfig>(configuration.GetSection("IngestionSettings"));
        services.AddSingleton<IIngestionSettingsProvider, IngestionSettingsProvider>();

        services.AddScoped<IFeedReader, FeedReader>();
        services.AddScoped<IFeedItemValidator, FeedItemValidator>();
        services.AddScoped<IFeedItemParser, FeedItemParser>();
        services.AddScoped<IFeedProcessor, FeedProcessingPipeline>();
        services.AddScoped<IFeedItemCleaner, FeedItemCleaner>();
        services.AddScoped<IFeedItemEnricher, FeedItemEnricher>();
        services.AddScoped<IImgTagExtractor, PreviewImgTagExtractor>();
        services.AddScoped<IPositivityAnalyzer, KeyPhrasePositivityAnalyzer>();

        services.AddScoped<IHtmlSanitizer, HtmlSanitizer>();
        services.AddScoped<IMediaEmbedder, MediaEmbedder>();
        services.AddScoped<ITextNormalizer, TextNormalizer>();
        services.AddScoped<ITopicNormalizer, TopicNormalizer>();

        services.AddHostedService<IngestionBackgroundService>();

        return services;
    }
}
