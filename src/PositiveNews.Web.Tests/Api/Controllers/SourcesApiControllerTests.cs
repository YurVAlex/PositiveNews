using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api;
using PositiveNews.Web.Api.Models;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class SourcesApiControllerTests
{
    [Fact]
    public async Task GetSources_Should_ReturnSources_When_QuerySucceeds()
    {
        IReadOnlyList<SourceFilterItemDto> items =
        [
            new() { Id = 1, Name = "Alpha News", LogoUrl = "https://alpha/logo.png" },
            new() { Id = 2, Name = "Beta Daily", LogoUrl = null }
        ];

        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetSourceFilterListQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(items));

        var sut = new SourcesApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        using var cts = new CancellationTokenSource();
        var result = await sut.GetSources(cts.Token);

        await mediator.Received(1).Send(Arg.Any<GetSourceFilterListQuery>(), cts.Token);
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<SourcesMetadataResponse>().Subject;
        body.Sources.Should().HaveCount(2);
        body.Sources[0].Id.Should().Be(1);
        body.Sources[0].Name.Should().Be("Alpha News");
        body.Sources[1].LogoUrl.Should().BeNull();
    }
}
