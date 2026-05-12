using FluentAssertions;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Domain.Tests.Entities;

public class UserSourceFilterTests
{
    [Fact]
    public void Create_Should_SetUserAndSourceIds_When_Called()
    {
        var f = UserSourceFilter.Create(10, 20);

        f.UserId.Should().Be(10);
        f.SourceId.Should().Be(20);
    }
}
