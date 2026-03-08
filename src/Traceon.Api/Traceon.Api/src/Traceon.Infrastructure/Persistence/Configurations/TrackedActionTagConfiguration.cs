using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class TrackedActionTagConfiguration : IEntityTypeConfiguration<TrackedActionTag>
{
    public void Configure(EntityTypeBuilder<TrackedActionTag> builder)
    {
        builder.ToTable("TrackedActionTags");

        builder.HasKey(e => new { e.TrackedActionId, e.TagId });

        builder.HasOne<Tag>()
            .WithMany()
            .HasForeignKey(e => e.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
