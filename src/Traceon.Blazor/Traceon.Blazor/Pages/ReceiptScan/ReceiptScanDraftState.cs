using System.Text.Json.Serialization;

namespace Traceon.Blazor.Pages.ReceiptScan;

/// <summary>
/// Serializable snapshot of the receipt scan wizard state.
/// Stored as JSON in the ReceiptScanDraft.SerializedState column.
/// </summary>
public sealed class ReceiptScanDraftState
{
    public int CurrentStep { get; set; }
    public Guid SelectedActionId { get; set; }
    public string SelectedActionName { get; set; } = string.Empty;

    // Scan result
    public string? MerchantName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? Tax { get; set; }
    public decimal? Total { get; set; }
    public double Confidence { get; set; }

    // Items with their current editing state
    public List<DraftReceiptItem> Items { get; set; } = [];

    // Field mapping sources (int = ReceiptFieldMappingSource enum value)
    public List<DraftFieldMapping> FieldMappings { get; set; } = [];
}

public sealed class DraftReceiptItem
{
    public string Description { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public bool Include { get; set; } = true;
    public Guid? MatchedRuleId { get; set; }

    /// <summary>FieldId → assigned category value.</summary>
    public Dictionary<string, string> CategoryValues { get; set; } = [];
}

public sealed class DraftFieldMapping
{
    public Guid FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Stored as int to avoid coupling to Blazor-only enum.</summary>
    public int Source { get; set; }

    public string? FixedValue { get; set; }
}
