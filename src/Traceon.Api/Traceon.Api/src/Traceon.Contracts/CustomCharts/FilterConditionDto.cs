using Traceon.Contracts.Enums;

namespace Traceon.Contracts.CustomCharts;

/// <summary>
/// A node in the composite filter tree. Contains leaf conditions and/or nested sub-groups.
/// Logic determines whether children are combined with AND or OR.
/// </summary>
public sealed record FilterGroupDto(
    FilterLogic Logic,
    List<FilterConditionDto>? Conditions,
    List<FilterGroupDto>? Groups);

/// <summary>
/// A leaf condition in the filter tree. References a field, an operator, and value(s).
/// <c>ValueTo</c> is used only for the <see cref="FilterOperator.Between"/> operator.
/// </summary>
public sealed record FilterConditionDto(
    Guid FieldId,
    FilterOperator Operator,
    string? Value,
    string? ValueTo);
