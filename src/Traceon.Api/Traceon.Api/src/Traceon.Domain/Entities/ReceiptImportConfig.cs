namespace Traceon.Domain.Entities;

public sealed class ReceiptImportConfig : Entity
{
    public Guid TrackedActionId { get; private set; }
    public Guid? ShopFieldId { get; private set; }
    public Guid? DescriptionFieldId { get; private set; }
    public Guid? TotalFieldId { get; private set; }
    public Guid? QuantityFieldId { get; private set; }
    public Guid? UnitPriceFieldId { get; private set; }

    /// <summary>JSON dictionary of fieldId to static value.</summary>
    public string? StaticFieldValues { get; private set; }

    private readonly List<ReceiptMappingRule> _mappingRules = [];
    public IReadOnlyCollection<ReceiptMappingRule> MappingRules => _mappingRules.AsReadOnly();

    private ReceiptImportConfig(Guid trackedActionId)
    {
        TrackedActionId = trackedActionId;
    }

    public static ReceiptImportConfig Create(Guid trackedActionId)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));

        return new ReceiptImportConfig(trackedActionId);
    }

    public void Update(
        Guid? shopFieldId,
        Guid? descriptionFieldId,
        Guid? totalFieldId,
        Guid? quantityFieldId,
        Guid? unitPriceFieldId,
        string? staticFieldValues = null)
    {
        ShopFieldId = shopFieldId;
        DescriptionFieldId = descriptionFieldId;
        TotalFieldId = totalFieldId;
        QuantityFieldId = quantityFieldId;
        UnitPriceFieldId = unitPriceFieldId;
        StaticFieldValues = staticFieldValues;
        MarkUpdated();
    }
}
