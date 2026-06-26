using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PositiveNews.Web.Api;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class AdminApiControllerTests
{
    [Fact]
    public void GetStatus_Should_ReturnOkPayload_When_Called()
    {
        var mediator = Substitute.For<IMediator>();
        var sut = new AdminApiController(mediator);

        var result = sut.GetStatus();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public void Controller_Should_HaveAuthorizeAdminRole_When_Declared()
    {
        var attr = typeof(AdminApiController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .Single();

        attr.Roles.Should().Be("Admin");
    }
}
