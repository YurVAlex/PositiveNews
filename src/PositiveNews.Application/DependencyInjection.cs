using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PositiveNews.Application.Services.Ingestion;

namespace PositiveNews.Application;

/// <summary>
/// Clean Registration Pattern for the Application layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<ITopicLookupBuilder, TopicLookupBuilder>();
        services.AddScoped<IArticleDeduplicator, ArticleDeduplicator>();

        return services;
    }
}
