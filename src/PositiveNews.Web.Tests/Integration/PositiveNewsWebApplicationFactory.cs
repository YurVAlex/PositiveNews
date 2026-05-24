using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Web;

namespace PositiveNews.Web.Tests.Integration;

/// <summary>
/// Boots the web app with environment <c>Testing</c> (skips DB seeding per <see cref="Program"/>).
/// Replaces ingestion run reads with a stub so authorization tests do not require LocalDB.
/// </summary>
internal sealed class PositiveNewsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IIngestionRunReadRepository>();

            var readRepository = Substitute.For<IIngestionRunReadRepository>();
            readRepository
                .GetLatestAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<IngestionRunListItemDto>>([]));

            services.AddScoped(_ => readRepository);
        });
    }
}
