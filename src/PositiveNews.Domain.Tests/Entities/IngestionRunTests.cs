using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Tests.Entities;

public class IngestionRunTests
{
    [Fact]
    public void Start_Should_CreateRunningRun_When_SourceIdProvided()
    {
        var run = IngestionRun.Start(1);

        run.SourceId.Should().Be(1);
        run.Status.Should().Be(IngestionStatus.Running);
        run.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        run.ItemsFetched.Should().Be(0);
    }

    [Fact]
    public void Complete_Should_SetSuccessAndItemCount_When_RunWasRunning()
    {
        var run = IngestionRun.Start(1);

        run.Complete(42);

        run.Status.Should().Be(IngestionStatus.Success);
        run.ItemsFetched.Should().Be(42);
        run.FinishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_Should_ThrowInvalidIngestionTransitionException_When_NotRunning()
    {
        var run = IngestionRun.Start(1);
        run.Complete(0);

        var act = () => run.Complete(0);

        act.Should().Throw<InvalidIngestionTransitionException>();
    }

    [Fact]
    public void PartialComplete_Should_SetPartialStatus_When_RunWasRunning()
    {
        var run = IngestionRun.Start(1);

        run.PartialComplete(5, "timeout");

        run.Status.Should().Be(IngestionStatus.Partial);
        run.ItemsFetched.Should().Be(5);
        run.ErrorMessage.Should().Be("timeout");
    }

    [Fact]
    public void PartialComplete_Should_ThrowInvalidIngestionTransitionException_When_NotRunning()
    {
        var run = IngestionRun.Start(1);
        run.PartialComplete(1);

        var act = () => run.PartialComplete(1);

        act.Should().Throw<InvalidIngestionTransitionException>();
    }

    [Fact]
    public void Fail_Should_TruncateErrorMessage_When_MessageLongerThan4000()
    {
        var run = IngestionRun.Start(1);
        var longMsg = new string('e', 5000);

        run.Fail(longMsg, 2);

        run.Status.Should().Be(IngestionStatus.Failed);
        run.ErrorMessage!.Length.Should().Be(4000);
        run.ItemsFetched.Should().Be(2);
    }

    [Fact]
    public void Fail_Should_ThrowInvalidIngestionTransitionException_When_NotRunning()
    {
        var run = IngestionRun.Start(1);
        run.Fail("first");

        var act = () => run.Fail("second");

        act.Should().Throw<InvalidIngestionTransitionException>();
    }
}
