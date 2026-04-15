using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class DropdownValueMetadataValueConfiguration : IEntityTypeConfiguration<DropdownValueMetadataValue>
{
    public void Configure(EntityTypeBuilder<DropdownValueMetadataValue> builder)
    {
        builder.ToTable("DropdownValueMetadataValues");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Value)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.DropdownValueId);
        builder.HasIndex(e => e.MetadataFieldId);

        builder.HasIndex(e => new { e.DropdownValueId, e.MetadataFieldId })
            .IsUnique();

        builder.HasOne<DropdownValue>()
            .WithMany()
            .HasForeignKey(e => e.DropdownValueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<DropdownValueMetadataField>()
            .WithMany()
            .HasForeignKey(e => e.MetadataFieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
