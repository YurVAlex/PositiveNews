using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core model configuration for <see cref="ArticleMetadata"/>.
/// </summary>
internal sealed class ArticleMetadataConfiguration : IEntityTypeConfiguration<ArticleMetadata>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleMetadata> builder)
    {
        builder.ToTable("ArticlesMetadata", SchemaNames.Catalog);

        builder.HasKey(a => a.Id);
        builder.Property(a => a.ExternalId).HasMaxLength(FieldLengths.Article.ExternalId);
        builder.Property(a => a.Title).HasMaxLength(FieldLengths.Article.Title).IsRequired();
        builder.Property(a => a.Author).HasMaxLength(FieldLengths.Article.Author);
        builder.Property(a => a.Url).HasMaxLength(FieldLengths.Article.Url).IsRequired();
        builder.Property(a => a.ImageTag).HasColumnType("nvarchar(max)");
        builder.Property(a => a.PublishedAt).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(a => a.IngestedAt).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(a => a.PositivityScore).HasColumnType("decimal(5,4)");
        builder.Property(a => a.ViewCount).HasDefaultValue(0L);
        builder.Property(a => a.LanguageCode).HasMaxLength(FieldLengths.Article.LanguageCode).HasDefaultValue(LanguageDefaults.Undetermined);
        builder.Property(a => a.RegionCode).HasMaxLength(FieldLengths.Article.RegionCode).HasDefaultValue(LanguageDefaults.GlobalRegion);
        builder.Property(a => a.IsActive).HasDefaultValue(true);
        builder.Property(a => a.SummaryShort).HasMaxLength(FieldLengths.Article.SummaryShort);

        // Check constraint kept as defense-in-depth alongside domain invariant
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

        // Backing field navigation access for collections
        builder.Navigation(a => a.ArticleTopics)
               .HasField("_articleTopics")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(a => a.Comments)
               .HasField("_comments")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
