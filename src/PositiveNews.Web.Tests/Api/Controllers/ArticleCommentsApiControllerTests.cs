using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PositiveNews.Application.Commands.Comments;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Comments;
using PositiveNews.Application.Queries.Comments;
using PositiveNews.Web.Api;
using PositiveNews.Web.Api.Models;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class ArticleCommentsApiControllerTests
{
    [Fact]
    public async Task GetComments_Should_ReturnComments_When_QuerySucceeds()
    {
        var comments = new List<CommentListItemDto>
        {
            new() { Id = 1, UserId = 2, UserName = "Alice", Content = "Nice", CreatedAt = DateTime.UtcNow }
        };
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetArticleCommentsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IReadOnlyList<CommentListItemDto>>.Success(comments)));

        var sut = new ArticleCommentsApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create()
        };

        var result = await sut.GetComments(10, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ArticleCommentsListResponse>().Subject;
        body.Comments.Should().HaveCount(1);
        body.Comments[0].UserName.Should().Be("Alice");
        await mediator.Received(1).Send(
            Arg.Is<GetArticleCommentsQuery>(q => q.ArticleId == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddComment_Should_ReturnCreated_When_CommandSucceeds()
    {
        var created = new CommentCreatedDto
        {
            Id = 7,
            UserId = 2,
            UserName = "Alice",
            Content = "Hello",
            CreatedAt = DateTime.UtcNow
        };
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<AddArticleCommentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<CommentCreatedDto>.Success(created)));

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "2")],
            "Test"));
        var sut = new ArticleCommentsApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(user, "POST")
        };

        var result = await sut.AddComment(
            10,
            new AddArticleCommentRequest { Content = "Hello" },
            CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeOfType<CommentResponse>();
        await mediator.Received(1).Send(
            Arg.Is<AddArticleCommentCommand>(c => c.ArticleId == 10 && c.UserId == 2 && c.Content == "Hello"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddComment_Should_ReturnUnauthorized_When_UserIdMissing()
    {
        var mediator = Substitute.For<IMediator>();
        var sut = new ArticleCommentsApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(method: "POST")
        };

        var result = await sut.AddComment(
            10,
            new AddArticleCommentRequest { Content = "Hello" },
            CancellationToken.None);

        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        await mediator.DidNotReceive().Send(Arg.Any<AddArticleCommentCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitComplaint_Should_ReturnNoContent_When_CommandSucceeds()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<SubmitCommentComplaintCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "3")],
            "Test"));
        var sut = new ArticleCommentsApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(user, "POST")
        };

        var result = await sut.SubmitComplaint(
            10,
            5,
            new SubmitCommentComplaintRequest { Reason = "Spam" },
            CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        await mediator.Received(1).Send(
            Arg.Is<SubmitCommentComplaintCommand>(c =>
                c.ArticleId == 10 && c.CommentId == 5 && c.UserId == 3 && c.Reason == "Spam"),
            Arg.Any<CancellationToken>());
    }
}
