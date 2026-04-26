using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class EntryTemplateFieldValueConfiguration : IEntityTypeConfiguration<EntryTemplateFieldValue>
{
    public void Configure(EntityTypeBuilder<EntryTemplateFieldValue> builder)
    {
        builder.ToTable("EntryTemplateFieldValues");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Order)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(e => e.EntryTemplateFieldId);
    }
}
