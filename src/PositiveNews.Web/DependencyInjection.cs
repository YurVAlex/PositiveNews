using PositiveNews.Application;
using PositiveNews.Infrastructure;
using PositiveNews.Web.Api.ExceptionHandling;
using PositiveNews.Web.Extensions;

namespace PositiveNews.Web;

/// <summary>
/// Web-layer service registration for API, SPA hosting, and cross-cutting concerns.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application, infrastructure, API, authentication, and documentation services.
    /// </summary>
    public static IServiceCollection AddWebServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddControllers();

        services.AddSwaggerDocumentation();
        services.AddJwtAuthentication(configuration);

        return services;
    }
}
