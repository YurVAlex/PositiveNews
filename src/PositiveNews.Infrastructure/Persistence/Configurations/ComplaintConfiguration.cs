using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core model configuration for <see cref="Complaint"/>.
/// </summary>
internal sealed class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.ToTable("Complains", SchemaNames.Community);

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Reason).HasMaxLength(500).IsRequired();
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(c => c.Comment)
               .WithMany(c => c.Complaints)
               .HasForeignKey(c => c.CommentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
               .WithMany()
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => new { c.UserId, c.CommentId })
               .IsUnique()
               .HasDatabaseName("IX_Complains_User_Comment");
    }
}
