namespace Traceon.Contracts.ReceiptImport;

public sealed record UpdateReceiptImportConfigRequest(
    Guid? ShopFieldId = null,
    Guid? DescriptionFieldId = null,
    Guid? TotalFieldId = null,
    Guid? QuantityFieldId = null,
    Guid? UnitPriceFieldId = null,
    Guid? DiscountFieldId = null,
    string? StaticFieldValues = null);
