using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Traceon.Contracts.FieldDefinitions;
using Traceon.Contracts.Enums;

namespace Traceon.Blazor.Components;

public sealed class FieldDefinitionModel
{
    [Required]
    public string DefaultName { get; set; } = string.Empty;
    public string? DefaultDescription { get; set; }
    public FieldType Type { get; set; } = FieldType.Text;
    public string? DropdownValues { get; set; }
    public bool DefaultIsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? Unit { get; set; }

    // Integer min/max
    public int? IntMinValue { get; set; }
    public int? IntMaxValue { get; set; }

    // Decimal min/max
    public decimal? DecimalMinValue { get; set; }
    public decimal? DecimalMaxValue { get; set; }

    // Date min/max
    public DateTime? MinDateValue { get; set; }
    public DateTime? MaxDateValue { get; set; }

    // Type-specific default value helpers
    public bool DefaultBoolValue { get; set; }
    public int? DefaultIntValue { get; set; }
    public decimal? DefaultDecimalValue { get; set; }
    public DateTime? DefaultDateValue { get; set; }

    public string[] ParsedDropdownValues =>
        string.IsNullOrWhiteSpace(DropdownValues)
            ? Array.Empty<string>()
            : DropdownValues.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(v => v, StringComparer.CurrentCultureIgnoreCase).ToArray();

    public void LoadTypedDefaults(decimal? apiMin, decimal? apiMax)
    {
        switch (Type)
        {
            case FieldType.Integer:
                IntMinValue = apiMin.HasValue ? (int)apiMin.Value : null;
                IntMaxValue = apiMax.HasValue ? (int)apiMax.Value : null;
                break;
            case FieldType.Decimal:
                DecimalMinValue = apiMin;
                DecimalMaxValue = apiMax;
                break;
        }

        if (string.IsNullOrWhiteSpace(DefaultValue)) return;

        switch (Type)
        {
            case FieldType.Boolean:
                DefaultBoolValue = DefaultValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case FieldType.Integer when int.TryParse(DefaultValue, CultureInfo.InvariantCulture, out var i):
                DefaultIntValue = i;
                break;
            case FieldType.Decimal when decimal.TryParse(DefaultValue, CultureInfo.InvariantCulture, out var d):
                DefaultDecimalValue = d;
                break;
            case FieldType.Date when DateTime.TryParse(DefaultValue, CultureInfo.InvariantCulture, out var dt):
                DefaultDateValue = dt;
                break;
        }
    }

    public string? ResolveDefaultValue() => Type switch
    {
        FieldType.Boolean => DefaultBoolValue ? "true" : "false",
        FieldType.Integer => DefaultIntValue?.ToString(CultureInfo.InvariantCulture),
        FieldType.Decimal => DefaultDecimalValue?.ToString(CultureInfo.InvariantCulture),
        FieldType.Date => DefaultDateValue?.ToString("yyyy-MM-dd"),
        _ => DefaultValue
    };

    public (decimal? Min, decimal? Max) ResolveMinMax() => Type switch
    {
        FieldType.Integer => (IntMinValue, IntMaxValue),
        FieldType.Decimal => (DecimalMinValue, DecimalMaxValue),
        _ => (null, null)
    };

    public IReadOnlyList<string> ValidateMinMax(IStringLocalizer L)
    {
        var errors = new List<string>();

        switch (Type)
        {
            case FieldType.Integer:
                if (IntMinValue.HasValue && IntMaxValue.HasValue && IntMinValue > IntMaxValue)
                    errors.Add(L["Validation_MinGreaterThanMax"]);
                if (DefaultIntValue.HasValue)
                {
                    if (IntMinValue.HasValue && DefaultIntValue < IntMinValue)
                        errors.Add(L["Validation_DefaultLessThanMin", IntMinValue]);
                    if (IntMaxValue.HasValue && DefaultIntValue > IntMaxValue)
                        errors.Add(L["Validation_DefaultGreaterThanMax", IntMaxValue]);
                }
                break;

            case FieldType.Decimal:
                if (DecimalMinValue.HasValue && DecimalMaxValue.HasValue && DecimalMinValue > DecimalMaxValue)
                    errors.Add(L["Validation_MinGreaterThanMax"]);
                if (DefaultDecimalValue.HasValue)
                {
                    if (DecimalMinValue.HasValue && DefaultDecimalValue < DecimalMinValue)
                        errors.Add(L["Validation_DefaultLessThanMin", DecimalMinValue]);
                    if (DecimalMaxValue.HasValue && DefaultDecimalValue > DecimalMaxValue)
                        errors.Add(L["Validation_DefaultGreaterThanMax", DecimalMaxValue]);
                }
                break;

            case FieldType.Date:
                if (MinDateValue.HasValue && MaxDateValue.HasValue && MinDateValue > MaxDateValue)
                    errors.Add(L["Validation_MinDateAfterMax"]);
                if (DefaultDateValue.HasValue)
                {
                    if (MinDateValue.HasValue && DefaultDateValue < MinDateValue)
                        errors.Add(L["Validation_DefaultDateBeforeMin", MinDateValue.Value.ToString("yyyy-MM-dd")]);
                    if (MaxDateValue.HasValue && DefaultDateValue > MaxDateValue)
                        errors.Add(L["Validation_DefaultDateAfterMax", MaxDateValue.Value.ToString("yyyy-MM-dd")]);
                }
                break;
        }

        return errors;
    }
}
