using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable("Sources", SchemaNames.Catalog);

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.BaseUrl).HasMaxLength(500).IsRequired();
        builder.Property(s => s.FeedUrl).HasMaxLength(500);
        builder.Property(s => s.LogoUrl).HasMaxLength(500);
        builder.Property(s => s.ApiEndpoint).HasMaxLength(500);
        builder.Property(s => s.ApiEncryptedKey).HasColumnType("nvarchar(max)");
        builder.Property(s => s.TrustScore).HasColumnType("decimal(5,2)").HasDefaultValue(1.0m);

        builder.ToTable(t => t.HasCheckConstraint("CK_Sources_Trust", "[TrustScore] >= 0.00"));

        builder.Property(s => s.DefaultLanguageCode).HasMaxLength(10).HasDefaultValue("en");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(s => s.IsActive).HasDefaultValue(true);

        builder.HasOne(s => s.Moderator)
               .WithMany()
               .HasForeignKey(s => s.ModeratedBy)
               .OnDelete(DeleteBehavior.NoAction);
    }
}