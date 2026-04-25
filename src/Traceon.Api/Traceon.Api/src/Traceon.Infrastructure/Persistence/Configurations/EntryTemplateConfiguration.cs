using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class EntryTemplateConfiguration : IEntityTypeConfiguration<EntryTemplate>
{
    public void Configure(EntityTypeBuilder<EntryTemplate> builder)
    {
        builder.ToTable("EntryTemplates");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.TrackedActionId);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.HasOne<TrackedAction>()
            .WithMany()
            .HasForeignKey(e => e.TrackedActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Fields)
            .WithOne()
            .HasForeignKey(f => f.EntryTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Fields).HasField("_fields");
    }
}
