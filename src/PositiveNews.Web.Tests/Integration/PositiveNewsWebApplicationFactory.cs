using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PositiveNews.Web;

namespace PositiveNews.Web.Tests.Integration;

/// <summary>
/// Boots the web app with environment <c>Testing</c> (skips DB seeding per <see cref="Program"/>).
/// </summary>
internal sealed class PositiveNewsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
