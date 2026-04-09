using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class ReceiptImportConfigConfiguration : IEntityTypeConfiguration<ReceiptImportConfig>
{
    public void Configure(EntityTypeBuilder<ReceiptImportConfig> builder)
    {
        builder.ToTable("ReceiptImportConfigs");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.TrackedActionId).IsUnique();

        builder.HasOne<TrackedAction>()
            .WithOne()
            .HasForeignKey<ReceiptImportConfig>(e => e.TrackedActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.ShopFieldId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.DescriptionFieldId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.TotalFieldId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.QuantityFieldId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<ActionField>()
            .WithMany()
            .HasForeignKey(e => e.UnitPriceFieldId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
