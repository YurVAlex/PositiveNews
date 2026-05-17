using FluentAssertions;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Domain.Tests.Entities;

public class UserTopicFilterTests
{
    [Fact]
    public void Create_Should_SetUserAndTopicIds_When_Called()
    {
        var f = UserTopicFilter.Create(10, 20);

        f.UserId.Should().Be(10);
        f.TopicId.Should().Be(20);
    }
}
