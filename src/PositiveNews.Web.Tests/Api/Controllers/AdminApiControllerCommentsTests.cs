using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PositiveNews.Application.Commands.Admin;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;
using PositiveNews.Web.Api;
using PositiveNews.Web.Api.Models;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class AdminApiControllerCommentsTests
{
    [Fact]
    public async Task GetCommentDetail_Should_ReturnDetail_When_QuerySucceeds()
    {
        var dto = new CommentAdminDetailDto
        {
            Id = 7,
            Content = "Hello",
            UserId = 2,
            UserName = "Jane",
            IsActive = true,
            ArticleId = 1,
        };

        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetAdminCommentDetailQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CommentAdminDetailDto>.Success(dto)));

        var sut = new AdminApiController(mediator);

        var result = await sut.GetCommentDetail(7, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<CommentAdminDetailResponse>().Subject;
        body.Id.Should().Be(7);
        body.Content.Should().Be("Hello");
        await mediator.Received(1).Send(
            Arg.Is<GetAdminCommentDetailQuery>(q => q.CommentId == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateComment_Should_ReturnNoContent_When_CommandSucceeds()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<ModerateCommentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "42")],
            "Test"));
        var sut = new AdminApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(user, "PUT")
        };

        var result = await sut.UpdateComment(
            7,
            new UpdateCommentRequest { IsActive = false, Reason = "spam", Note = "reviewed" },
            CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        await mediator.Received(1).Send(
            Arg.Is<ModerateCommentCommand>(c =>
                c.CommentId == 7
                && c.IsActive == false
                && c.Reason == "spam"
                && c.Note == "reviewed"
                && c.ModeratorId == 42),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateComment_Should_ReturnUnauthorized_When_ModeratorIdMissing()
    {
        var mediator = Substitute.For<IMediator>();
        var sut = new AdminApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(method: "PUT")
        };

        var result = await sut.UpdateComment(
            7,
            new UpdateCommentRequest { IsActive = false },
            CancellationToken.None);

        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        await mediator.DidNotReceive().Send(Arg.Any<ModerateCommentCommand>(), Arg.Any<CancellationToken>());
    }
}
