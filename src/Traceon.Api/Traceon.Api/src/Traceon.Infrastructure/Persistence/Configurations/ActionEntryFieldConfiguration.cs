using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class ActionEntryFieldConfiguration : IEntityTypeConfiguration<ActionEntryField>
{
    public void Configure(EntityTypeBuilder<ActionEntryField> builder)
    {
        builder.ToTable("ActionEntryFields");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Value)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.ActionEntryId);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.ActionFieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
