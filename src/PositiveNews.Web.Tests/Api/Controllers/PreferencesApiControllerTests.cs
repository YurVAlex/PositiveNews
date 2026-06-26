using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PositiveNews.Application.Commands.FeedPreferences;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.FeedPreferences;
using PositiveNews.Application.Queries.FeedPreferences;
using PositiveNews.Web.Api;
using PositiveNews.Web.Api.Models;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class PreferencesApiControllerTests
{
    [Fact]
    public async Task GetFeedPreferences_Should_ReturnPreferences_When_QuerySucceeds()
    {
        var dto = new UserFeedPreferencesDto(["Health"], [1], 0.6m, "date");
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetUserFeedPreferencesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<UserFeedPreferencesDto>.Success(dto)));

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "42")],
            "Test"));
        var sut = new PreferencesApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(user)
        };

        var result = await sut.GetFeedPreferences(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<UserFeedPreferencesResponse>().Subject;
        body.TopicNames.Should().Contain("Health");
        body.SourceIds.Should().Contain(1);
        await mediator.Received(1).Send(
            Arg.Is<GetUserFeedPreferencesQuery>(q => q.UserId == 42),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PutFeedPreferences_Should_ReturnPreferences_When_CommandSucceeds()
    {
        var dto = new UserFeedPreferencesDto(["Science"], [2], 0.8m, "preferences");
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<UpdateUserFeedPreferencesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<UserFeedPreferencesDto>.Success(dto)));

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "7")],
            "Test"));
        var sut = new PreferencesApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(user, "PUT")
        };

        var result = await sut.PutFeedPreferences(
            new UpdateUserFeedPreferencesRequest
            {
                TopicNames = ["Science"],
                SourceIds = [2],
                MinPositivity = 0.8m,
                SortBy = "preferences"
            },
            CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<UserFeedPreferencesResponse>();
        await mediator.Received(1).Send(
            Arg.Is<UpdateUserFeedPreferencesCommand>(c =>
                c.UserId == 7 && c.SortBy == "preferences" && c.MinPositivity == 0.8m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFeedPreferences_Should_ReturnUnauthorized_When_UserIdMissing()
    {
        var mediator = Substitute.For<IMediator>();
        var sut = new PreferencesApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create()
        };

        var result = await sut.GetFeedPreferences(CancellationToken.None);

        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        await mediator.DidNotReceive().Send(Arg.Any<GetUserFeedPreferencesQuery>(), Arg.Any<CancellationToken>());
    }
}
