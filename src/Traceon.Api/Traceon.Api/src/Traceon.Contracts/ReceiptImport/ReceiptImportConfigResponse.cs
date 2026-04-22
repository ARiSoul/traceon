namespace Traceon.Contracts.ReceiptImport;

public sealed record ReceiptImportConfigResponse
{
    public required Guid Id { get; init; }
    public required Guid TrackedActionId { get; init; }
    public required Guid? ShopFieldId { get; init; }
    public required Guid? DescriptionFieldId { get; init; }
    public required Guid? TotalFieldId { get; init; }
    public required Guid? QuantityFieldId { get; init; }
    public required Guid? UnitPriceFieldId { get; init; }
    public required Guid? DiscountFieldId { get; init; }
    public Guid? ReceiptDiscountTypeFieldId { get; init; }
    public string? ReceiptDiscountTypeValue { get; init; }
    public string? StaticFieldValues { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}
