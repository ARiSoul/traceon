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

        builder.HasIndex(e => e.TrackedActionId);

        builder.HasOne<FieldDefinition>()
            .WithMany()
            .HasForeignKey(e => e.FieldDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
