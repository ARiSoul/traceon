using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class ReceiptMappingRuleConfiguration : IEntityTypeConfiguration<ReceiptMappingRule>
{
    public void Configure(EntityTypeBuilder<ReceiptMappingRule> builder)
    {
        builder.ToTable("ReceiptMappingRules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Pattern)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(e => e.ReceiptImportConfigId);

        builder.HasOne<ReceiptImportConfig>()
            .WithMany(c => c.MappingRules)
            .HasForeignKey(e => e.ReceiptImportConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.TargetFieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
