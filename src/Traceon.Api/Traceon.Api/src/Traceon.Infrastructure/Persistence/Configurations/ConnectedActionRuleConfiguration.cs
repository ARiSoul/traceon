using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class ConnectedActionRuleConfiguration : IEntityTypeConfiguration<ConnectedActionRule>
{
    public void Configure(EntityTypeBuilder<ConnectedActionRule> builder)
    {
        builder.ToTable("ConnectedActionRules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.ConditionsJson)
            .HasMaxLength(4000);

        builder.Property(e => e.FieldMappingsJson)
            .HasMaxLength(4000);

        builder.Property(e => e.CopyNotes)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CopyDate)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(e => e.SourceTrackedActionId);

        builder.HasOne<TrackedAction>()
            .WithMany(a => a.ConnectedActionRules)
            .HasForeignKey(e => e.SourceTrackedActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TrackedAction>()
            .WithMany()
            .HasForeignKey(e => e.TargetTrackedActionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
