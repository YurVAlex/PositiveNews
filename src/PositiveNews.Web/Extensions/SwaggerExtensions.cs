using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PositiveNews.Web.Extensions;

/// <summary>
/// OpenAPI / Swagger registration and middleware for the REST API.
/// </summary>
public static class SwaggerExtensions
{
    public const string DocumentName = "v1";
    private const string ApiTitle = "PositiveNews API";

    /// <summary>
    /// Registers Swagger generation with JWT bearer security metadata.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(ConfigureSwagger);
        return services;
    }

    /// <summary>
    /// Enables Swagger UI in development.
    /// </summary>
    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{DocumentName}/swagger.json", $"{ApiTitle} {DocumentName}");
            options.DocumentTitle = ApiTitle;
        });

        return app;
    }

    private static void ConfigureSwagger(SwaggerGenOptions options)
    {
        options.SwaggerDoc(
            DocumentName,
            new OpenApiInfo
            {
                Title = ApiTitle,
                Version = DocumentName,
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
    }
}
