using FluentAssertions;
using PositiveNews.Application.Commands.FeedPreferences;

namespace PositiveNews.Application.Tests.FeedPreferences;

public class UpdateUserFeedPreferencesCommandValidatorTests
{
    private readonly UpdateUserFeedPreferencesCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Fail_When_MinPositivityOutOfRange()
    {
        var result = _validator.Validate(new UpdateUserFeedPreferencesCommand(
            1,
            [],
            [],
            1.5m,
            "date"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserFeedPreferencesCommand.MinPositivity));
    }

    [Fact]
    public void Validate_Should_Fail_When_SortByInvalid()
    {
        var result = _validator.Validate(new UpdateUserFeedPreferencesCommand(
            1,
            [],
            [],
            0.5m,
            "invalid"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserFeedPreferencesCommand.SortBy));
    }

    [Fact]
    public void Validate_Should_Pass_For_ValidSnapshot()
    {
        var result = _validator.Validate(new UpdateUserFeedPreferencesCommand(
            1,
            ["Health"],
            [2],
            0.6m,
            "positivity"));

        result.IsValid.Should().BeTrue();
    }
}
