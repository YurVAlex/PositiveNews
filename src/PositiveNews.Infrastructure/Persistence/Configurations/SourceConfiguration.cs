using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core model configuration for <see cref="Source"/>.
/// </summary>
internal sealed class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    /// <inheritdoc />
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

        // Defense-in-depth check constraint
        builder.ToTable(t => t.HasCheckConstraint("CK_Sources_Trust", "[TrustScore] >= 0.00"));

        builder.Property(s => s.DefaultLanguageCode).HasMaxLength(10).HasDefaultValue("en");
        builder.Property(s => s.DefaultThumbnailHtml).HasColumnType("nvarchar(max)");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(s => s.IsActive).HasDefaultValue(true);

        builder.HasOne(s => s.Moderator)
               .WithMany()
               .HasForeignKey(s => s.ModeratedBy)
               .OnDelete(DeleteBehavior.NoAction);

        // Backing field navigation access for collections
        builder.Navigation(s => s.Articles)
               .HasField("_articles")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(s => s.IngestionRuns)
               .HasField("_ingestionRuns")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(s => s.UserSourceFilters)
               .HasField("_userSourceFilters")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
