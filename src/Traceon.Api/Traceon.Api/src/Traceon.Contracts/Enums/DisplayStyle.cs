namespace Traceon.Contracts.Enums;

/// <summary>
/// How an ActionField's value is rendered in the entry editor and lists. Each FieldType has its
/// own valid subset; <see cref="Default"/> always falls back to the framework-native control.
/// </summary>
public enum DisplayStyle
{
    Default = 0,

    // ── Numeric (Integer, Decimal) ──
    Rating = 1,
    ProgressBar = 2,
    Slider = 3,
    IconRepeat = 10,

    // ── Boolean ──
    Checkbox = 4,
    IconPair = 5,

    // ── Dropdown / CompositeDropdown ──
    Chips = 6,
    Radio = 7,

    // ── Text ──
    Textarea = 8,
    Markdown = 9,
}
