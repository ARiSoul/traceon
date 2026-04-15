namespace Traceon.Domain.Entities;

public sealed class DropdownValue : Entity
{
    public Guid FieldDefinitionId { get; private set; }
    public string Value { get; private set; }
    public int SortOrder { get; private set; }

    private DropdownValue(Guid fieldDefinitionId, string value, int sortOrder)
    {
        FieldDefinitionId = fieldDefinitionId;
        Value = value;
        SortOrder = sortOrder;
    }

    public static DropdownValue Create(Guid fieldDefinitionId, string value, int sortOrder = 0)
    {
        if (fieldDefinitionId == Guid.Empty)
            throw new ArgumentException("Field definition ID is required.", nameof(fieldDefinitionId));

        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new DropdownValue(fieldDefinitionId, value.Trim(), sortOrder);
    }

    public void Rename(string newValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newValue);
        Value = newValue.Trim();
        MarkUpdated();
    }

    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        MarkUpdated();
    }
}