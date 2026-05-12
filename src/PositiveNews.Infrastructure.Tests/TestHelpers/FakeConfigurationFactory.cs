using Microsoft.Extensions.Configuration;
using PositiveNews.Infrastructure.Security;

namespace PositiveNews.Infrastructure.Tests.TestHelpers;

internal static class FakeConfigurationFactory
{
    public static IConfiguration CreateJwtOnly(JwtOptions overrides)
    {
        var dict = new Dictionary<string, string?>
        {
            [$"{JwtOptions.SectionName}:Issuer"] = overrides.Issuer,
            [$"{JwtOptions.SectionName}:Audience"] = overrides.Audience,
            [$"{JwtOptions.SectionName}:SecretKey"] = overrides.SecretKey,
            [$"{JwtOptions.SectionName}:AccessTokenMinutes"] = overrides.AccessTokenMinutes.ToString()
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }
}
