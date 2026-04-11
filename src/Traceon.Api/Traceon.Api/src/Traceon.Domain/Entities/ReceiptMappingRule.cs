namespace Traceon.Domain.Entities;

public sealed class ReceiptMappingRule : Entity
{
    public Guid ReceiptImportConfigId { get; private set; }
    public Guid TargetFieldId { get; private set; }
    public string Pattern { get; private set; }
    public string Value { get; private set; }
    public int Priority { get; private set; }

    private ReceiptMappingRule(
        Guid receiptImportConfigId,
        Guid targetFieldId,
        string pattern,
        string value,
        int priority)
    {
        ReceiptImportConfigId = receiptImportConfigId;
        TargetFieldId = targetFieldId;
        Pattern = pattern;
        Value = value;
        Priority = priority;
    }

    public static ReceiptMappingRule Create(
        Guid receiptImportConfigId,
        Guid targetFieldId,
        string pattern,
        string value,
        int priority = 0)
    {
        if (receiptImportConfigId == Guid.Empty)
            throw new ArgumentException("Receipt import config ID is required.", nameof(receiptImportConfigId));

        if (targetFieldId == Guid.Empty)
            throw new ArgumentException("Target field ID is required.", nameof(targetFieldId));

        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new ReceiptMappingRule(receiptImportConfigId, targetFieldId, pattern.Trim(), value.Trim(), priority);
    }

    public void Update(string? pattern = null, string? value = null, int? priority = null)
    {
        if (pattern is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
            Pattern = pattern.Trim();
        }

        if (value is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            Value = value.Trim();
        }

        if (priority.HasValue)
            Priority = priority.Value;

        MarkUpdated();
    }
}
