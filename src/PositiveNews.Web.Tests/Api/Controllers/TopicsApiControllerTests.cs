using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api;
using PositiveNews.Web.Api.Models;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class TopicsApiControllerTests
{
    [Fact]
    public async Task GetTopics_Should_ReturnTopicNames_When_QuerySucceeds()
    {
        IReadOnlyList<string> names = ["Alpha", "Beta"];
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetTopicFilterListQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(names));

        var sut = new TopicsApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        using var cts = new CancellationTokenSource();
        var result = await sut.GetTopics(cts.Token);

        await mediator.Received(1).Send(Arg.Any<GetTopicFilterListQuery>(), cts.Token);
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<TopicsMetadataResponse>().Subject;
        body.TopicNames.Should().BeEquivalentTo(names);
    }
}
