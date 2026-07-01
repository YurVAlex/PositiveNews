using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PositiveNews.Application.Common.Behaviors;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using PositiveNews.Application.Services.Ingestion;

namespace PositiveNews.Application;

/// <summary>
/// Clean Registration Pattern for the Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR, FluentValidation, validation pipeline behavior, and ingestion helpers.
    /// </summary>
    /// <param name="services">The application's dependency injection container.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<ITopicLookupBuilder, TopicLookupBuilder>();
        services.AddScoped<IArticleDeduplicator, ArticleDeduplicator>();

        return services;
    }
}
