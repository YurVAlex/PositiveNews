using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;



namespace PositiveNews.Infrastructure.Tests.DependencyInjection;



public class HttpClientRegistrationTests

{

    [Fact]

    public void RssFeedClient_Should_HaveTimeoutAndHeaders_When_CreatedFromFactory()

    {

        var services = new ServiceCollection();

        services.AddHttpClient("RssFeedClient", client =>

        {

            client.Timeout = TimeSpan.FromSeconds(30);

            client.DefaultRequestHeaders.UserAgent.ParseAdd(

                "PositiveNews/1.0 (+https://github.com/positivenews; Academic Project)");

            client.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/xml, text/xml");

        });



        using var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var client = factory.CreateClient("RssFeedClient");



        client.Timeout.Should().Be(TimeSpan.FromSeconds(30));

        client.DefaultRequestHeaders.UserAgent.ToString().Should().Contain("PositiveNews");

        client.DefaultRequestHeaders.Accept.ToString().Should().Contain("xml");

    }

}

