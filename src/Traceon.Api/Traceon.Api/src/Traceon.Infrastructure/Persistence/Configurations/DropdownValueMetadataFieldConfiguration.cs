using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class DropdownValueMetadataFieldConfiguration : IEntityTypeConfiguration<DropdownValueMetadataField>
{
    public void Configure(EntityTypeBuilder<DropdownValueMetadataField> builder)
    {
        builder.ToTable("DropdownValueMetadataFields");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.MinValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.MaxValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.DefaultValue)
            .HasMaxLength(500);

        builder.Property(e => e.Unit)
            .HasMaxLength(20);

        builder.Property(e => e.DropdownValues)
            .HasMaxLength(2000);

        builder.Property(e => e.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.DisplayStyle)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(e => e.FieldDefinitionId);

        builder.HasIndex(e => new { e.FieldDefinitionId, e.Name })
            .IsUnique();

        builder.HasOne<FieldDefinition>()
            .WithMany()
            .HasForeignKey(e => e.FieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
