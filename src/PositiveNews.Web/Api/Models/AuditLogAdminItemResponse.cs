using System;
using System.Text.Json.Serialization;
using PositiveNews.Domain.Enums;

namespace PositiveNews.Web.Api.Models;

public sealed class AuditLogAdminItemResponse
{
    public long Id { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AuditEntityType EntityType { get; init; }

    public long EntityId { get; init; }
    public string? ChangedField { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public long ModeratorId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? Reason { get; init; }
    public string? Note { get; init; }
}
