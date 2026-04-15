using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class DropdownValueConfiguration : IEntityTypeConfiguration<DropdownValue>
{
    public void Configure(EntityTypeBuilder<DropdownValue> builder)
    {
        builder.ToTable("DropdownValues");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(e => e.FieldDefinitionId);

        builder.HasIndex(e => new { e.FieldDefinitionId, e.Value })
            .IsUnique();
    }
}