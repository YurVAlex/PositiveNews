using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", SchemaNames.Admin);

        builder.HasKey(al => al.Id);

        builder.Property(al => al.EntityType)
               .HasMaxLength(50)
               .HasConversion<string>();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Audit_Entity", "[EntityType] IN ('Article', 'Comment', 'User', 'Source')"));

        builder.Property(al => al.ChangedField).HasMaxLength(100);
        builder.Property(al => al.OldValue).HasColumnType("nvarchar(max)");
        builder.Property(al => al.NewValue).HasColumnType("nvarchar(max)");
        builder.Property(al => al.Reason).HasMaxLength(500);
        builder.Property(al => al.Note).HasMaxLength(1000);
        builder.Property(al => al.CreatedAt).HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(al => al.Moderator)
               .WithMany()
               .HasForeignKey(al => al.ModeratorId)
               .OnDelete(DeleteBehavior.NoAction);

        // History lookup index
        builder.HasIndex(al => new { al.EntityType, al.EntityId, al.CreatedAt })
               .IsDescending(false, false, true)
               .HasDatabaseName("IX_AuditLogs_Entity_History");
    }
}