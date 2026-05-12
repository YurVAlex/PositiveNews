using System.Security.Claims;

namespace PositiveNews.Web.Tests.TestHelpers;

internal static class FakeUser
{
    public static ClaimsPrincipal Admin(string userId = "1") =>
        WithRoles(userId, "Admin");

    public static ClaimsPrincipal Standard(string userId = "2") =>
        WithRoles(userId, "User");

    public static ClaimsPrincipal WithRoles(string userId, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, authenticationType: "TestScheme");
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());
}
