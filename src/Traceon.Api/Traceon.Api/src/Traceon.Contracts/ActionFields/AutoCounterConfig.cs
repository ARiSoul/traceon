using Traceon.Contracts.Enums;

namespace Traceon.Contracts.ActionFields;

/// <summary>
/// Per-ActionField configuration for the AutoCounter <see cref="InitialValueBehavior"/>.
/// On entry creation, when the submitted value for the field is null/empty, the API computes
/// previous + Step (or StartValue if no prior matching entry exists in this tracked action).
/// Conditions filter which prior entries count as "matching"; an empty Conditions list means
/// every prior entry of the tracked action matches.
/// </summary>
public sealed record AutoCounterConfig(
    decimal Step,
    decimal StartValue,
    FilterLogic ConditionLogic,
    List<AutoCounterCondition>? Conditions);

/// <summary>
/// A single condition node. Operator is type-aware on the UI side; Value is the string-encoded
/// comparand (matches how ActionEntryField stores values).
///
/// When <see cref="UseCurrentValue"/> is true, the calculator ignores <see cref="Value"/> and
/// matches against the in-progress entry's value for this field — so a single AutoCounter rule
/// can express "increment per (show, season)" instead of one literal rule per group. Operator is
/// implicitly Equals in that mode.
/// </summary>
public sealed record AutoCounterCondition(
    Guid FieldId,
    FilterOperator Operator,
    string? Value,
    bool UseCurrentValue = false);
