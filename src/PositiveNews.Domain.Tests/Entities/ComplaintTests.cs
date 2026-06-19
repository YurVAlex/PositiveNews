using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Tests.Entities;

public class ComplaintTests
{
    [Fact]
    public void Create_Should_BuildComplaint_When_ValidInput()
    {
        var complaint = Complaint.Create(1, 5, "Inappropriate content");

        complaint.UserId.Should().Be(1);
        complaint.CommentId.Should().Be(5);
        complaint.Reason.Should().Be("Inappropriate content");
        complaint.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_Should_TrimReason_When_InputHasWhitespace()
    {
        var complaint = Complaint.Create(1, 5, "  Spam  ");

        complaint.Reason.Should().Be("Spam");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_ReasonEmpty(string? reason)
    {
        var act = () => Complaint.Create(1, 5, reason!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Should_ThrowDomainException_When_ReasonExceeds500Characters()
    {
        var longReason = new string('x', 501);

        var act = () => Complaint.Create(1, 5, longReason);

        act.Should().Throw<DomainException>();
    }
}
