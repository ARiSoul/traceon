using Traceon.Contracts.ReceiptImport;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class ReceiptImportMappingExtensions
{
    public static ReceiptImportConfigResponse ToResponse(this ReceiptImportConfig entity) =>
        new()
        {
            Id = entity.Id,
            TrackedActionId = entity.TrackedActionId,
            ShopFieldId = entity.ShopFieldId,
            DescriptionFieldId = entity.DescriptionFieldId,
            TotalFieldId = entity.TotalFieldId,
            QuantityFieldId = entity.QuantityFieldId,
            UnitPriceFieldId = entity.UnitPriceFieldId,
            DiscountFieldId = entity.DiscountFieldId,
            StaticFieldValues = entity.StaticFieldValues,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    public static ReceiptMappingRuleResponse ToResponse(this ReceiptMappingRule entity, string targetFieldName) =>
        new()
        {
            Id = entity.Id,
            ReceiptImportConfigId = entity.ReceiptImportConfigId,
            TargetFieldId = entity.TargetFieldId,
            TargetFieldName = targetFieldName,
            Pattern = entity.Pattern,
            Value = entity.Value,
            Priority = entity.Priority,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}
