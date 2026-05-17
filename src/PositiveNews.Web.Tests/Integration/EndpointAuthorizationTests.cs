using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Integration;

public class EndpointAuthorizationTests
{
    [Fact]
    public async Task Me_Should_Return401_When_NoBearerToken()
    {
        using var factory = new PositiveNewsWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminStatus_Should_Return401_When_NoBearerToken()
    {
        using var factory = new PositiveNewsWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/status");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminStatus_Should_Return403_When_UserRoleOnly()
    {
        using var factory = new PositiveNewsWebApplicationFactory();
        var client = factory.CreateClient();
        var token = JwtTokenFactory.CreateAccessToken("1", ["User"]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/admin/status");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminStatus_Should_Return200_When_AdminRole()
    {
        using var factory = new PositiveNewsWebApplicationFactory();
        var client = factory.CreateClient();
        var token = JwtTokenFactory.CreateAccessToken("1", ["Admin"]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/admin/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ArticlesFeed_Should_NotReturn401_When_Unauthenticated()
    {
        using var factory = new PositiveNewsWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/articles/feed?page=1");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Topics_Should_NotReturn401_When_Unauthenticated()
    {
        using var factory = new PositiveNewsWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/topics");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
