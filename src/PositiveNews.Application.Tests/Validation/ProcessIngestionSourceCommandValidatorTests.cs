using FluentAssertions;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Tests.TestSupport;

namespace PositiveNews.Application.Tests.Validation;

public class ProcessIngestionSourceCommandValidatorTests
{
    private readonly ProcessIngestionSourceCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_ThrowNullReferenceException_When_SourceIsNull()
    {
        var cmd = new ProcessIngestionSourceCommand(null!, IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings());

        var act = () => _validator.Validate(cmd);

        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void Validate_Should_Fail_When_SourceIdNotPositive()
    {
        var source = IngestionTestData.ValidSource(0);
        var cmd = new ProcessIngestionSourceCommand(source, IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings());

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Id"));
    }

    [Theory]
    [InlineData("", "https://x.com/f", "en")]
    [InlineData("Name", "", "en")]
    [InlineData("Name", "https://x.com/f", "")]
    public void Validate_Should_Fail_When_RequiredSourceFieldsEmpty(string name, string feedUrl, string lang)
    {
        var source = new IngestionSourceSnapshot(1, name, feedUrl, lang, null);
        var cmd = new ProcessIngestionSourceCommand(source, IngestionTestData.EmptyTopicLookup(), IngestionTestData.MinimalSettings());

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_TopicLookupNull()
    {
        var cmd = new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), null!, IngestionTestData.MinimalSettings());

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ProcessIngestionSourceCommand.TopicLookup));
    }

    [Fact]
    public void Validate_Should_Fail_When_IngestionSettingsNull()
    {
        var cmd = new ProcessIngestionSourceCommand(IngestionTestData.ValidSource(), IngestionTestData.EmptyTopicLookup(), null!);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ProcessIngestionSourceCommand.IngestionSettings));
    }

    [Fact]
    public void Validate_Should_Succeed_When_CommandWellFormed()
    {
        var cmd = new ProcessIngestionSourceCommand(
            IngestionTestData.ValidSource(),
            IngestionTestData.EmptyTopicLookup(),
            IngestionTestData.MinimalSettings());

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }
}
