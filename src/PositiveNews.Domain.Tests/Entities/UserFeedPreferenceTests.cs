using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Tests.Entities;

public class UserFeedPreferenceTests
{
    [Fact]
    public void Create_Should_SetDefaults_When_OnlyUserIdProvided()
    {
        var pref = UserFeedPreference.Create(1);

        pref.UserId.Should().Be(1);
        pref.MinPositivity.Should().Be(0.5m);
        pref.SortBy.Should().Be("Date");
    }

    [Fact]
    public void Create_Should_ThrowDomainException_When_MinPositivityOutOfRange()
    {
        var act = () => UserFeedPreference.Create(1, minPositivity: 1.5m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdatePreferences_Should_OverwriteFields_When_ValidInput()
    {
        var pref = UserFeedPreference.Create(1);

        pref.UpdatePreferences(0.8m, "Popularity", "en", "US");

        pref.MinPositivity.Should().Be(0.8m);
        pref.SortBy.Should().Be("Popularity");
        pref.LanguageCode.Should().Be("en");
        pref.RegionCode.Should().Be("US");
    }
}
