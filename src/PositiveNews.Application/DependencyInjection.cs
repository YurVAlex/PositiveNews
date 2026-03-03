using Microsoft.Extensions.DependencyInjection;

namespace PositiveNews.Application;

/// <summary>
/// Clean Registration Pattern for the Application layer.
/// Will host service registrations when Application-level services appear.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Future: AutoMapper profiles, FluentValidation validators, etc.
        return services;
    }
}