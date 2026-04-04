using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class FieldAnalyticsRuleConfiguration : IEntityTypeConfiguration<FieldAnalyticsRule>
{
    public void Configure(EntityTypeBuilder<FieldAnalyticsRule> builder)
    {
        builder.ToTable("FieldAnalyticsRules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FilterValue)
            .HasMaxLength(500);

        builder.Property(e => e.Label)
            .HasMaxLength(200);

        builder.Property(e => e.Aggregation)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.DisplayType)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.NegativeValues)
            .HasMaxLength(500);

        builder.HasIndex(e => e.TrackedActionId);

        builder.HasOne<TrackedAction>()
            .WithMany(a => a.AnalyticsRules)
            .HasForeignKey(e => e.TrackedActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.MeasureFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.GroupByFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.FilterFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.SignFieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
