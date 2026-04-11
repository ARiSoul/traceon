using Traceon.Contracts.Enums;

namespace Traceon.Blazor.Components;

public enum ReceiptFieldMappingSource
{
    None,
    ReceiptDescription,
    ReceiptQuantity,
    ReceiptUnitPrice,
    ReceiptTotal,
    ReceiptDiscount,
    MerchantName,
    FixedValue
}

public sealed class ReceiptFieldMapping
{
    public Guid FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public ReceiptFieldMappingSource Source { get; set; }
    public string? FixedValue { get; set; }
}
