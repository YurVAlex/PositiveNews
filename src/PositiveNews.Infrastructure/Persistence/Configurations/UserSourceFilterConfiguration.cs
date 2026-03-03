using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class UserSourceFilterConfiguration : IEntityTypeConfiguration<UserSourceFilter>
{
    public void Configure(EntityTypeBuilder<UserSourceFilter> builder)
    {
        builder.ToTable("UserSourceFilters", SchemaNames.Identity);

        builder.HasKey(usf => new { usf.UserId, usf.SourceId });

        builder.HasOne(usf => usf.User)
               .WithMany(u => u.SourceFilters)
               .HasForeignKey(usf => usf.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(usf => usf.Source)
               .WithMany(s => s.UserSourceFilters)
               .HasForeignKey(usf => usf.SourceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}