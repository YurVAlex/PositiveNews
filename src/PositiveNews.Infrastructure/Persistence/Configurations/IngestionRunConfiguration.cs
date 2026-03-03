using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class IngestionRunConfiguration : IEntityTypeConfiguration<IngestionRun>
{
    public void Configure(EntityTypeBuilder<IngestionRun> builder)
    {
        builder.ToTable("IngestionRuns", SchemaNames.Catalog);

        builder.HasKey(ir => ir.Id);

        // Store enum as string for readability and to match the SQL CHECK constraint.
        builder.Property(ir => ir.Status)
               .HasMaxLength(50)
               .HasConversion<string>();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Ingestion_Status", "[Status] IN ('Running', 'Success', 'Failed', 'Partial')"));

        builder.Property(ir => ir.ItemsFetched).HasDefaultValue(0);
        builder.Property(ir => ir.ErrorMessage).HasColumnType("nvarchar(max)");

        builder.HasOne(ir => ir.Source)
               .WithMany(s => s.IngestionRuns)
               .HasForeignKey(ir => ir.SourceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}