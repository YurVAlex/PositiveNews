using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core model configuration for <see cref="RefreshToken"/>.
/// </summary>
internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", SchemaNames.Identity);

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(rt => rt.UserId)
               .IsRequired();

        builder.HasOne(rt => rt.User)
               .WithMany()
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(rt => rt.ExpiresAtUtc)
               .IsRequired();

        builder.Property(rt => rt.CreatedAtUtc)
               .IsRequired();

        builder.Property(rt => rt.IsRevoked)
               .IsRequired();

        builder.Property(rt => rt.RevokedAtUtc)
               .IsRequired(false);

        builder.HasIndex(rt => rt.Token)
               .IsUnique();
    }
}
