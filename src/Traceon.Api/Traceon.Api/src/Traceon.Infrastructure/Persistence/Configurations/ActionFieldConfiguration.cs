using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class ActionFieldConfiguration : IEntityTypeConfiguration<ActionField>
{
    public void Configure(EntityTypeBuilder<ActionField> builder)
    {
        builder.ToTable("ActionFields");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.MaxValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.MinValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.DefaultValue)
            .HasMaxLength(500);

        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("UN");

        builder.Property(e => e.Order)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.SummaryMetrics)
            .IsRequired()
            .HasDefaultValue(63);

        builder.Property(e => e.TrendAggregation)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.TrendChartType)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.TargetValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.InitialValueBehavior)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.InitialValuePeriodUnit)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.InitialValuePeriodCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.DropdownTrendValueFieldId)
            .IsRequired(false);

        builder.Property(e => e.DropdownTrendAggregation)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.DropdownTrendChartType)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.AutoCounterConfigJson)
            .HasMaxLength(4000);

        builder.Property(e => e.IsMultiselect)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(e => e.TrackedActionId);

        builder.HasOne<FieldDefinition>()
            .WithMany()
            .HasForeignKey(e => e.FieldDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
