using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", SchemaNames.Identity);

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(300).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.EmailConfirmed).HasDefaultValue(false);
        builder.Property(u => u.Name).HasMaxLength(200).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnType("nvarchar(max)");
        builder.Property(u => u.FailedLoginCount).HasDefaultValue(0);
        builder.Property(u => u.AvatarPictureUrl).HasMaxLength(1000);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("sysutcdatetime()");
        builder.Property(u => u.IsActive).HasDefaultValue(true);

        builder.HasOne(u => u.Moderator)
               .WithMany()
               .HasForeignKey(u => u.ModeratedBy)
               .OnDelete(DeleteBehavior.NoAction);
    }
}