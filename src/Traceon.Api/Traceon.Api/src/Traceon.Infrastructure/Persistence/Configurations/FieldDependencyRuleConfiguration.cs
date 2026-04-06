using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class FieldDependencyRuleConfiguration : IEntityTypeConfiguration<FieldDependencyRule>
{
    public void Configure(EntityTypeBuilder<FieldDependencyRule> builder)
    {
        builder.ToTable("FieldDependencyRules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SourceValue)
            .HasMaxLength(500);

        builder.Property(e => e.RuleType)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.TargetConstraint)
            .HasMaxLength(2000);

        builder.Property(e => e.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(e => e.TrackedActionId);

        builder.HasOne<TrackedAction>()
            .WithMany(a => a.DependencyRules)
            .HasForeignKey(e => e.TrackedActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.SourceFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.TargetFieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
