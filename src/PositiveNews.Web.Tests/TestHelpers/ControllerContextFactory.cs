using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PositiveNews.Web.Tests.TestHelpers;

internal static class ControllerContextFactory
{
    public static ControllerContext Create(
        ClaimsPrincipal? user = null,
        string method = "GET",
        string path = "/")
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = method,
                Path = path
            },
            User = user ?? new ClaimsPrincipal(new ClaimsIdentity())
        };

        return new ControllerContext { HttpContext = httpContext };
    }
}
