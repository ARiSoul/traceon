using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class ReceiptScanDraftConfiguration : IEntityTypeConfiguration<ReceiptScanDraft>
{
    public void Configure(EntityTypeBuilder<ReceiptScanDraft> builder)
    {
        builder.ToTable("ReceiptScanDrafts");

        builder.HasKey(e => e.Id);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasIndex(e => e.UserId);

        builder.Property(e => e.UserId).HasMaxLength(450).IsRequired();
        builder.Property(e => e.MerchantName).HasMaxLength(500);
        builder.Property(e => e.SelectedActionName).HasMaxLength(500);
        builder.Property(e => e.SerializedState).IsRequired();
        builder.Property(e => e.Total).HasPrecision(18, 2);
    }
}
