using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using PositiveNews.Application.Common;
using PositiveNews.Application.Common.Behaviors;

namespace PositiveNews.Application.Tests.Common;

/// <summary>Public so NSubstitute can implement <see cref="IValidator{T}"/> for pipeline tests.</summary>
public sealed record ValidationPipelineTestRequest(string Name);

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_Should_InvokeNext_When_NoValidators()
    {
        var behavior = new ValidationBehavior<ValidationPipelineTestRequest, Result>([]);
        var called = false;

        var result = await behavior.Handle(
            new ValidationPipelineTestRequest("ok"),
            _ =>
            {
                called = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_InvokeNext_When_AllValidatorsSucceed()
    {
        var v1 = Substitute.For<IValidator<ValidationPipelineTestRequest>>();
        v1.ValidateAsync(Arg.Any<ValidationContext<ValidationPipelineTestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var v2 = Substitute.For<IValidator<ValidationPipelineTestRequest>>();
        v2.ValidateAsync(Arg.Any<ValidationContext<ValidationPipelineTestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<ValidationPipelineTestRequest, Result>([v1, v2]);
        var called = false;

        var result = await behavior.Handle(
            new ValidationPipelineTestRequest("ok"),
            _ =>
            {
                called = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
        await v1.Received(1).ValidateAsync(Arg.Any<ValidationContext<ValidationPipelineTestRequest>>(), Arg.Any<CancellationToken>());
        await v2.Received(1).ValidateAsync(Arg.Any<ValidationContext<ValidationPipelineTestRequest>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationFailure_When_SingleRuleFails()
    {
        var validator = Substitute.For<IValidator<ValidationPipelineTestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidationPipelineTestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Name is required.")]));
        var behavior = new ValidationBehavior<ValidationPipelineTestRequest, Result>([validator]);
        var called = false;

        var result = await behavior.Handle(
            new ValidationPipelineTestRequest(""),
            _ =>
            {
                called = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be(ErrorCodes.Validation.Failed);
        result.Error.Message.Should().Be("Name is required.");
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_AggregateMessages_When_MultipleFailures()
    {
        var validator = Substitute.For<IValidator<ValidationPipelineTestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidationPipelineTestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(
            [
                new ValidationFailure("A", "First error."),
                new ValidationFailure("B", "Second error.")
            ]));
        var behavior = new ValidationBehavior<ValidationPipelineTestRequest, Result>([validator]);
        var called = false;

        var result = await behavior.Handle(
            new ValidationPipelineTestRequest("x"),
            _ =>
            {
                called = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("First error. Second error.");
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_ReturnGenericResultFailure_When_ValidationFails()
    {
        var validator = Substitute.For<IValidator<ValidationPipelineTestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidationPipelineTestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Name is required.")]));
        var behavior = new ValidationBehavior<ValidationPipelineTestRequest, Result<int>>([validator]);
        var called = false;

        var result = await behavior.Handle(
            new ValidationPipelineTestRequest(""),
            _ =>
            {
                called = true;
                return Task.FromResult(Result<int>.Success(1));
            },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_When_ResponseIsNotResult()
    {
        var validator = Substitute.For<IValidator<ValidationPipelineTestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidationPipelineTestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Name is required.")]));
        var behavior = new ValidationBehavior<ValidationPipelineTestRequest, string>([validator]);
        var called = false;

        var act = () => behavior.Handle(
            new ValidationPipelineTestRequest(""),
            _ =>
            {
                called = true;
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        called.Should().BeFalse();
    }

}
