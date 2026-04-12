using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}