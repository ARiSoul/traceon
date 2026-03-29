using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Traceon.Infrastructure.Audit;

internal sealed class UserAuditLogConfiguration : IEntityTypeConfiguration<UserAuditLog>
{
    public void Configure(EntityTypeBuilder<UserAuditLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).HasMaxLength(450);
        builder.Property(e => e.UserEmail).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(64);
        builder.Property(e => e.Details).HasMaxLength(4000);
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(512);
        builder.Property(e => e.OccurredAtUtc).IsRequired();

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.OccurredAtUtc);
        builder.HasIndex(e => new { e.UserId, e.Action });
    }
}
