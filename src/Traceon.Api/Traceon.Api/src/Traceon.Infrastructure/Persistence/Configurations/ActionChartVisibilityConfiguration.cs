using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class ActionChartVisibilityConfiguration : IEntityTypeConfiguration<ActionChartVisibility>
{
    public void Configure(EntityTypeBuilder<ActionChartVisibility> builder)
    {
        builder.ToTable("ActionChartVisibilities");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.HiddenKeys)
            .HasMaxLength(4000);

        builder.Property(e => e.ChartOrder)
            .HasMaxLength(4000)
            .HasDefaultValue(string.Empty);

        builder.HasIndex(e => new { e.UserId, e.TrackedActionId }).IsUnique();

        builder.HasOne<TrackedAction>()
            .WithMany()
            .HasForeignKey(e => e.TrackedActionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
