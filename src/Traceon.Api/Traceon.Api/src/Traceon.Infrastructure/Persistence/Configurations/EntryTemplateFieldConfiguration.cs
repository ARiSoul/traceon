using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class EntryTemplateFieldConfiguration : IEntityTypeConfiguration<EntryTemplateField>
{
    public void Configure(EntityTypeBuilder<EntryTemplateField> builder)
    {
        builder.ToTable("EntryTemplateFields");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.EntryTemplateId);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.ActionFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Values)
            .WithOne()
            .HasForeignKey(v => v.EntryTemplateFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Values).HasField("_values");
    }
}
