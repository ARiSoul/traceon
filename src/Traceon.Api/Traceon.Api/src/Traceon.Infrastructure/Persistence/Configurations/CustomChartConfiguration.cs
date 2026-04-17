using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class CustomChartConfiguration : IEntityTypeConfiguration<CustomChart>
{
    public void Configure(EntityTypeBuilder<CustomChart> builder)
    {
        builder.ToTable("CustomCharts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Aggregation)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.ChartType)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.TimeGrouping)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.SortDescending)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.ShowTotalizer)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.ColorPalette)
            .HasMaxLength(500);

        builder.Property(e => e.FilterConditionsJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(e => e.TrackedActionId);

        builder.HasOne<TrackedAction>()
            .WithMany()
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
    }
}
