using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Traceon.Infrastructure.Identity;

internal sealed class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("UserRefreshTokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired().HasMaxLength(450);
        builder.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
        builder.Property(e => e.ExpiresAtUtc).IsRequired();
        builder.Property(e => e.CreatedAtUtc).IsRequired();

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.TokenHash);
        builder.HasIndex(e => e.ExpiresAtUtc);
    }
}
