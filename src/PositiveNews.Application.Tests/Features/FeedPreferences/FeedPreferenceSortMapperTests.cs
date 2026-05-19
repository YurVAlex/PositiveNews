using FluentAssertions;
using PositiveNews.Application.Features.FeedPreferences;

namespace PositiveNews.Application.Tests.Features.FeedPreferences;

public class FeedPreferenceSortMapperTests
{
    [Theory]
    [InlineData("Date", "date")]
    [InlineData("Positivity", "positivity")]
    [InlineData("Preferences", "preferences")]
    public void ToApiSort_Should_MapStoredValues(string stored, string expected)
    {
        FeedPreferenceSortMapper.ToApiSort(stored).Should().Be(expected);
    }

    [Theory]
    [InlineData("positivity", "Positivity")]
    [InlineData("preferences", "Preferences")]
    [InlineData("date", "Date")]
    public void ToStoredSort_Should_MapApiValues(string api, string expected)
    {
        FeedPreferenceSortMapper.ToStoredSort(api).Should().Be(expected);
    }
}
