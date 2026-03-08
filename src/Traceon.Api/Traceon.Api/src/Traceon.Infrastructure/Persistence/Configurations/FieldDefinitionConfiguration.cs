using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class FieldDefinitionConfiguration : IEntityTypeConfiguration<FieldDefinition>
{
    public void Configure(EntityTypeBuilder<FieldDefinition> builder)
    {
        builder.ToTable("FieldDefinitions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.DefaultName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.DefaultDescription)
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.DropdownValues)
            .HasMaxLength(2000);

        builder.Property(e => e.DefaultMaxValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.DefaultMinValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.DefaultValue)
            .HasMaxLength(500);

        builder.HasIndex(e => e.UserId);
    }
}
