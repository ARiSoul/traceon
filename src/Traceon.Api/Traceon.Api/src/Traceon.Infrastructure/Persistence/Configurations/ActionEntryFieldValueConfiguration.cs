using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class ActionEntryFieldValueConfiguration : IEntityTypeConfiguration<ActionEntryFieldValue>
{
    public void Configure(EntityTypeBuilder<ActionEntryFieldValue> builder)
    {
        builder.ToTable("ActionEntryFieldValues");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Order)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(e => e.ActionEntryFieldId);

        builder.HasIndex(e => e.Value);
    }
}
