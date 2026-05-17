using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PositiveNews.Web.Tests.Integration;

namespace PositiveNews.Web.Tests.DependencyInjection;

public class WebRegistrationTests
{
    [Fact]
    public void Host_Should_RegisterAuthenticationAndAuthorization_When_BuiltForTesting()
    {
        using var factory = new PositiveNewsWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IAuthorizationService>().Should().NotBeNull();
    }
}
