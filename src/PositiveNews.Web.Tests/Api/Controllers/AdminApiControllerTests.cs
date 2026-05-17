using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Web.Api;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class AdminApiControllerTests
{
    [Fact]
    public void GetStatus_Should_ReturnOkPayload_When_Called()
    {
        var sut = new AdminApiController();

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
