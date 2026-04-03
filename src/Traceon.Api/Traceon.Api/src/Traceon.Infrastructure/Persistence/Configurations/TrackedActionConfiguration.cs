using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class TrackedActionConfiguration : IEntityTypeConfiguration<TrackedAction>
{
    public void Configure(EntityTypeBuilder<TrackedAction> builder)
    {
        builder.ToTable("TrackedActions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.SortOrder)
            .HasDefaultValue(0);

        builder.HasIndex(e => new { e.UserId, e.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasMany(e => e.Fields)
            .WithOne()
            .HasForeignKey(f => f.TrackedActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Tags)
            .WithOne()
            .HasForeignKey(t => t.TrackedActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Fields).HasField("_fields");
        builder.Navigation(e => e.Tags).HasField("_tags");
    }
}
