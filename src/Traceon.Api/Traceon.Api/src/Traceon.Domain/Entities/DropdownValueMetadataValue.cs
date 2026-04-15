namespace Traceon.Domain.Entities;

public sealed class DropdownValueMetadataValue : Entity
{
    public Guid DropdownValueId { get; private set; }
    public Guid MetadataFieldId { get; private set; }
    public string? Value { get; private set; }

    private DropdownValueMetadataValue(Guid dropdownValueId, Guid metadataFieldId, string? value)
    {
        DropdownValueId = dropdownValueId;
        MetadataFieldId = metadataFieldId;
        Value = value;
    }

    public static DropdownValueMetadataValue Create(Guid dropdownValueId, Guid metadataFieldId, string? value)
    {
        if (dropdownValueId == Guid.Empty)
            throw new ArgumentException("Dropdown value ID is required.", nameof(dropdownValueId));

        if (metadataFieldId == Guid.Empty)
            throw new ArgumentException("Metadata field ID is required.", nameof(metadataFieldId));

        return new DropdownValueMetadataValue(dropdownValueId, metadataFieldId, value?.Trim());
    }

    public void UpdateValue(string? value)
    {
        Value = value?.Trim();
        MarkUpdated();
    }
}
