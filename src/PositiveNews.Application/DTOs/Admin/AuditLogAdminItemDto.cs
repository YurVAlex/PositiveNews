using System;
using PositiveNews.Domain.Enums;

namespace PositiveNews.Application.DTOs.Admin;

public sealed class AuditLogAdminItemDto
{
    public long Id { get; init; }
    public AuditEntityType EntityType { get; init; }
    public long EntityId { get; init; }
    public string? ChangedField { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string? Reason { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
    public long ModeratorId { get; init; }
}
