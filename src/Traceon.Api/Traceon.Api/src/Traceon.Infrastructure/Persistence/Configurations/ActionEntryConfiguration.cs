using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class ActionEntryConfiguration : IEntityTypeConfiguration<ActionEntry>
{
    public void Configure(EntityTypeBuilder<ActionEntry> builder)
    {
        builder.ToTable("ActionEntries");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.TrackedActionId);

        builder.HasIndex(e => e.OccurredAtUtc);

        builder.HasIndex(e => e.ReceiptImportBatchId);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.HasOne<TrackedAction>()
            .WithMany()
            .HasForeignKey(e => e.TrackedActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Fields)
            .WithOne()
            .HasForeignKey(f => f.ActionEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Fields).HasField("_fields");
    }
}
