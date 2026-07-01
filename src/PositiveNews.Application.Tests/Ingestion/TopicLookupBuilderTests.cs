using FluentAssertions;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Services.Ingestion;

namespace PositiveNews.Application.Tests.Ingestion;

public class TopicLookupBuilderTests
{
    private readonly TopicLookupBuilder _sut = new();

    [Fact]
    public void Build_Should_IndexByCanonicalNameCaseInsensitive()
    {
        var topic = new TopicSnapshot(1, "Health", "Good-News|Wellness, wins", null);

        var lookup = _sut.Build([topic]);

        lookup.ByName.ContainsKey("health").Should().BeTrue();
        lookup.ByName["HEALTH"].Should().Be(topic);
    }

    [Fact]
    public void Build_Should_SplitSlugWordsAndMatchSlugWordsCaseInsensitive()
    {
        var topic = new TopicSnapshot(1, "Health", "Good-News|Wellness, wins", null);

        var lookup = _sut.Build([topic]);

        lookup.BySlugWord["good-news"].Should().ContainSingle().Which.Should().Be(topic);
        lookup.BySlugWord["WELLNESS"].Should().ContainSingle().Which.Should().Be(topic);
        lookup.ChildToParentTopics["wins"].Should().ContainSingle().Which.Should().Be(topic);
    }
}
