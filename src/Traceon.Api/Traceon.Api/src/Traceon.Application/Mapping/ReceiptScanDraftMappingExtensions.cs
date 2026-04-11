using Traceon.Contracts.ReceiptScanDraft;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class ReceiptScanDraftMappingExtensions
{
    public static ReceiptScanDraftResponse ToResponse(this ReceiptScanDraft entity) =>
        new()
        {
            Id = entity.Id,
            MerchantName = entity.MerchantName,
            TransactionDate = entity.TransactionDate,
            Total = entity.Total,
            CurrentStep = entity.CurrentStep,
            SelectedActionId = entity.SelectedActionId,
            SelectedActionName = entity.SelectedActionName,
            SerializedState = entity.SerializedState,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}
