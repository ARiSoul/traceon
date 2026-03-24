using System.ComponentModel.DataAnnotations;
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
            : DropdownValues.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
            case FieldType.Integer when int.TryParse(DefaultValue, out var i):
                DefaultIntValue = i;
                break;
            case FieldType.Decimal when decimal.TryParse(DefaultValue, out var d):
                DefaultDecimalValue = d;
                break;
            case FieldType.Date when DateTime.TryParse(DefaultValue, out var dt):
                DefaultDateValue = dt;
                break;
        }
    }

    public string? ResolveDefaultValue() => Type switch
    {
        FieldType.Boolean => DefaultBoolValue ? "true" : "false",
        FieldType.Integer => DefaultIntValue?.ToString(),
        FieldType.Decimal => DefaultDecimalValue?.ToString(),
        FieldType.Date => DefaultDateValue?.ToString("yyyy-MM-dd"),
        _ => DefaultValue
    };

    public (decimal? Min, decimal? Max) ResolveMinMax() => Type switch
    {
        FieldType.Integer => (IntMinValue, IntMaxValue),
        FieldType.Decimal => (DecimalMinValue, DecimalMaxValue),
        _ => (null, null)
    };

    public IReadOnlyList<string> ValidateMinMax()
    {
        var errors = new List<string>();

        switch (Type)
        {
            case FieldType.Integer:
                if (IntMinValue.HasValue && IntMaxValue.HasValue && IntMinValue > IntMaxValue)
                    errors.Add("Min value cannot be greater than max value.");
                if (DefaultIntValue.HasValue)
                {
                    if (IntMinValue.HasValue && DefaultIntValue < IntMinValue)
                        errors.Add($"Default value cannot be less than min value ({IntMinValue}).");
                    if (IntMaxValue.HasValue && DefaultIntValue > IntMaxValue)
                        errors.Add($"Default value cannot be greater than max value ({IntMaxValue}).");
                }
                break;

            case FieldType.Decimal:
                if (DecimalMinValue.HasValue && DecimalMaxValue.HasValue && DecimalMinValue > DecimalMaxValue)
                    errors.Add("Min value cannot be greater than max value.");
                if (DefaultDecimalValue.HasValue)
                {
                    if (DecimalMinValue.HasValue && DefaultDecimalValue < DecimalMinValue)
                        errors.Add($"Default value cannot be less than min value ({DecimalMinValue}).");
                    if (DecimalMaxValue.HasValue && DefaultDecimalValue > DecimalMaxValue)
                        errors.Add($"Default value cannot be greater than max value ({DecimalMaxValue}).");
                }
                break;

            case FieldType.Date:
                if (MinDateValue.HasValue && MaxDateValue.HasValue && MinDateValue > MaxDateValue)
                    errors.Add("Min date cannot be later than max date.");
                if (DefaultDateValue.HasValue)
                {
                    if (MinDateValue.HasValue && DefaultDateValue < MinDateValue)
                        errors.Add($"Default date cannot be earlier than min date ({MinDateValue:yyyy-MM-dd}).");
                    if (MaxDateValue.HasValue && DefaultDateValue > MaxDateValue)
                        errors.Add($"Default date cannot be later than max date ({MaxDateValue:yyyy-MM-dd}).");
                }
                break;
        }

        return errors;
    }
}
