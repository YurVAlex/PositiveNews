using FluentAssertions;
using PositiveNews.Application.DTOs;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class TopicNormalizerTests
{
    private readonly TopicNormalizer _sut = new();

    [Fact]
    public void NormalizeTopics_Should_ReturnEmpty_When_InputNullOrEmpty()
    {
        _sut.NormalizeTopics([], EmptyLookup()).Should().BeEmpty();
    }

    [Fact]
    public void NormalizeTopics_Should_MapCaseInsensitiveSlug_When_LookupMatches()
    {
        var topic = new TopicSnapshot(1, "Climate", "climate", null);
        var lookup = new TopicLookup(
            new Dictionary<string, TopicSnapshot>(StringComparer.OrdinalIgnoreCase) { ["Climate"] = topic },
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase)
            {
                ["climate"] = new List<TopicSnapshot> { topic }
            },
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase));

        var result = _sut.NormalizeTopics(["CLIMATE news"], lookup);

        result.Should().Contain("Climate");
    }

    private static TopicLookup EmptyLookup()
        => new(
            new Dictionary<string, TopicSnapshot>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase));
}
