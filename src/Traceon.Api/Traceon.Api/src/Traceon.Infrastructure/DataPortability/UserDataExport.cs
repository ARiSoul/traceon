using System.Text.Json.Serialization;
using Traceon.Contracts.Enums;

namespace Traceon.Infrastructure.DataPortability;

public sealed class UserDataExport
{
    public string Version { get; init; } = "1.0";
    public DateTime ExportedAtUtc { get; init; } = DateTime.UtcNow;
    public string Email { get; init; } = string.Empty;

    public List<TagExport> Tags { get; init; } = [];
    public List<FieldDefinitionExport> FieldDefinitions { get; init; } = [];
    public List<TrackedActionExport> TrackedActions { get; init; } = [];
}

public sealed class TagExport
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Color { get; init; } = "#000000";
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class FieldDefinitionExport
{
    public Guid Id { get; init; }
    public string DefaultName { get; init; } = string.Empty;
    public string? DefaultDescription { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FieldType Type { get; init; }

    public string? DropdownValues { get; init; }
    public decimal? DefaultMaxValue { get; init; }
    public decimal? DefaultMinValue { get; init; }
    public bool DefaultIsRequired { get; init; }
    public string? DefaultValue { get; init; }
    public string Unit { get; init; } = "UN";
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class TrackedActionExport
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public List<Guid> TagIds { get; init; } = [];
    public List<ActionFieldExport> Fields { get; init; } = [];
    public List<ActionEntryExport> Entries { get; init; } = [];
}

public sealed class ActionFieldExport
{
    public Guid Id { get; init; }
    public Guid FieldDefinitionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? MaxValue { get; init; }
    public decimal? MinValue { get; init; }
    public bool IsRequired { get; init; }
    public string? DefaultValue { get; init; }
    public string Unit { get; init; } = "UN";
    public int Order { get; init; }
    public int SummaryMetrics { get; init; }
    public int TrendAggregation { get; init; }
    public int TrendChartType { get; init; }
    public decimal? TargetValue { get; init; }
}

public sealed class ActionEntryExport
{
    public Guid Id { get; init; }
    public DateTime OccurredAtUtc { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public List<EntryFieldExport> Fields { get; init; } = [];
}

public sealed class EntryFieldExport
{
    public Guid ActionFieldId { get; init; }
    public string? Value { get; init; }
}
