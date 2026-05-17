using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Tests.Entities;

public class AuditLogTests
{
    [Fact]
    public void Create_Should_SetAllFields_When_ValidInputProvided()
    {
        var log = AuditLog.Create(
            AuditEntityType.Article,
            entityId: 99,
            moderatorId: 5,
            changedField: "IsActive",
            oldValue: "true",
            newValue: "false",
            reason: "spam",
            note: "flagged");

        log.EntityType.Should().Be(AuditEntityType.Article);
        log.EntityId.Should().Be(99);
        log.ModeratorId.Should().Be(5);
        log.ChangedField.Should().Be("IsActive");
        log.OldValue.Should().Be("true");
        log.NewValue.Should().Be("false");
        log.Reason.Should().Be("spam");
        log.Note.Should().Be("flagged");
        log.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_Should_ThrowDomainException_When_ModeratorIdInvalid()
    {
        var act = () => AuditLog.Create(AuditEntityType.Article, 1, -1);

        act.Should().Throw<DomainException>();
    }
}
