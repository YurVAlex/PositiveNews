using FluentAssertions;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_ResultIsFailure()
    {
        var result = Result<int>.Failure(new Error("Code", "Message", ErrorType.Validation));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access Value for failed Result.");
    }

    [Fact]
    public void Map_Should_TransformValue_When_Success()
    {
        var result = Result<int>.Success(2)
            .Map(x => x + 3)
            .Map(x => $"value:{x}");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value:5");
    }

    [Fact]
    public void Map_Should_PreserveError_When_Failure()
    {
        var error = new Error("Broken", "No value", ErrorType.Unexpected);
        var failure = Result<int>.Failure(error).Map(x => x + 1);

        failure.IsFailure.Should().BeTrue();
        failure.Error.Should().Be(error);
    }

    [Fact]
    public void Bind_Should_ChainSuccess_When_BothStepsSucceed()
    {
        var result = Result<int>.Success(2)
            .Bind(x => Result<string>.Success($"n:{x}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("n:2");
    }

    [Fact]
    public void Bind_Should_ShortCircuit_When_FirstStepFails()
    {
        var err = new Error("E1", "fail", ErrorType.Unexpected);
        var result = Result<int>.Failure(err)
            .Bind(x => Result<string>.Success(x.ToString()));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(err);
    }

    [Fact]
    public void Match_Should_SelectBranch_BySuccess()
    {
        var ok = Result<int>.Success(3).Match(x => x * 2, _ => 0);
        var bad = Result<int>.Failure(new Error("x", "m", ErrorType.Validation))
            .Match(x => x, e => e.Code.Length);

        ok.Should().Be(6);
        bad.Should().Be(1);
    }

    [Fact]
    public void ImplicitOperator_Should_LiftErrorToResultT()
    {
        Error err = new("C", "M", ErrorType.NotFound);
        Result<int> r = err;

        r.IsFailure.Should().BeTrue();
        r.Error.Should().Be(err);
    }
}
