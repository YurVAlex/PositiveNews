using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;
using PositiveNews.Web.Api;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class AdminUsersApiControllerTests
{
    [Fact]
    public async Task GetUsers_Should_ReturnOk_WithUsers()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAdminUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new UserAdminItemDto
                {
                    Id = 1,
                    Name = "Jane",
                    IsActive = true,
                    EmailConfirmed = false,
                    FailedLoginCount = 0,
                    CreatedAt = DateTime.UtcNow
                }
            });

        var sut = new AdminApiController(mediator);

        var result = await sut.GetUsers(null, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }
}