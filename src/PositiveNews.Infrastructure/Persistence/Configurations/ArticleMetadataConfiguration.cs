using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class ArticleMetadataConfiguration : IEntityTypeConfiguration<ArticleMetadata>
{
    public void Configure(EntityTypeBuilder<ArticleMetadata> builder)
    {
        builder.ToTable("ArticlesMetadata", SchemaNames.Catalog);

        builder.HasKey(a => a.Id);
        builder.Property(a => a.ExternalId).HasMaxLength(300);
        builder.Property(a => a.Title).HasMaxLength(500).IsRequired();
        builder.Property(a => a.Author).HasMaxLength(300);
        builder.Property(a => a.Url).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.ImageUrl).HasMaxLength(1000);
        builder.Property(a => a.PublishedAt).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(a => a.IngestedAt).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(a => a.PositivityScore).HasColumnType("decimal(5,4)");
        builder.Property(a => a.ViewCount).HasDefaultValue(0L);
        builder.Property(a => a.LanguageCode).HasMaxLength(10).HasDefaultValue("und");
        builder.Property(a => a.RegionCode).HasMaxLength(10).HasDefaultValue("Global");
        builder.Property(a => a.IsActive).HasDefaultValue(true);

        // Check constraint
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Articles_Positivity", "[PositivityScore] BETWEEN 0.0000 AND 1.0000"));

        // Deduplication unique index
        builder.HasIndex(a => new { a.SourceId, a.ExternalId })
               .IsUnique()
               .HasFilter("[ExternalId] IS NOT NULL")
               .HasDatabaseName("IX_ArticlesMeta_Source_ExternalId");

        // Feed performance index
        builder.HasIndex(a => new { a.LanguageCode, a.RegionCode, a.PublishedAt })
               .IsDescending(false, false, true)
               .HasFilter("[IsActive] = 1")
               .HasDatabaseName("IX_ArticlesMeta_Feed_Date");

        // Source lookup index
        builder.HasIndex(a => a.SourceId)
               .HasDatabaseName("IX_ArticlesMeta_SourceId");

        // Relationships
        builder.HasOne(a => a.Source)
               .WithMany(s => s.Articles)
               .HasForeignKey(a => a.SourceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Moderator)
               .WithMany()
               .HasForeignKey(a => a.ModeratedBy)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.Content)
               .WithOne(c => c.Metadata)
               .HasForeignKey<ArticleContent>(c => c.Id)
               .OnDelete(DeleteBehavior.Cascade);
    }
}