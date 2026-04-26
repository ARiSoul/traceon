using System.Text.Json.Serialization;
using Traceon.Contracts.Enums;

namespace Traceon.Infrastructure.DataPortability;

public sealed class UserDataExport
{
    public string Version { get; set; } = "1.0";
    public DateTime ExportedAtUtc { get; set; } = DateTime.UtcNow;
    public string Email { get; set; } = string.Empty;

    public List<TagExport> Tags { get; set; } = [];
    public List<FieldDefinitionExport> FieldDefinitions { get; set; } = [];
    public List<TrackedActionExport> TrackedActions { get; set; } = [];
}

public sealed class TagExport
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#000000";
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class FieldDefinitionExport
{
    public Guid Id { get; set; }
    public string DefaultName { get; set; } = string.Empty;
    public string? DefaultDescription { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FieldType Type { get; set; }

    public string? DropdownValues { get; set; }
    public decimal? DefaultMaxValue { get; set; }
    public decimal? DefaultMinValue { get; set; }
    public bool DefaultIsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string Unit { get; set; } = "UN";
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class TrackedActionExport
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<Guid> TagIds { get; set; } = [];
    public List<ActionFieldExport> Fields { get; set; } = [];
    public List<ActionEntryExport> Entries { get; set; } = [];
}

public sealed class ActionFieldExport
{
    public Guid Id { get; set; }
    public Guid FieldDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? MinValue { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string Unit { get; set; } = "UN";
    public int Order { get; set; }
    public int SummaryMetrics { get; set; }
    public int TrendAggregation { get; set; }
    public int TrendChartType { get; set; }
    public decimal? TargetValue { get; set; }
    public int InitialValueBehavior { get; set; }
    public int InitialValuePeriodUnit { get; set; }
    public int InitialValuePeriodCount { get; set; }
    public Guid? DropdownTrendValueFieldId { get; set; }
    public int DropdownTrendAggregation { get; set; }
    public int DropdownTrendChartType { get; set; }
}

public sealed class ActionEntryExport
{
    public Guid Id { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<EntryFieldExport> Fields { get; set; } = [];
}

public sealed class EntryFieldExport
{
    public Guid ActionFieldId { get; set; }
    public List<string>? Values { get; set; }
    /// <summary>Legacy single-value field, kept so older export files still import correctly.</summary>
    public string? Value { get; set; }
}
