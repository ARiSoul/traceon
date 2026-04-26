using System.Globalization;
using Microsoft.Extensions.Logging;
using Traceon.Application.Mapping;
using Traceon.Contracts.ActionFields;
using Traceon.Contracts.Enums;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

/// <summary>
/// Computes the next AutoCounter value for an ActionField. Used both at entry-creation time
/// (server-side auto-fill when no value is submitted) and via the preview endpoint (live UI hint).
///
/// Algorithm:
///   1. Find the most recent prior entry of the same TrackedAction whose field values satisfy
///      the rule's conditions, evaluated against the in-progress submission.
///   2. If found, return previous numeric value + Step.
///   3. If no prior match, return StartValue.
/// </summary>
public interface IAutoCounterCalculator
{
    Task<decimal?> ComputeAsync(
        ActionField targetField,
        Guid trackedActionId,
        IReadOnlyDictionary<Guid, string?> currentValues,
        CancellationToken cancellationToken = default);
}

public sealed class AutoCounterCalculator(
    IActionEntryRepository entryRepository,
    ILogger<AutoCounterCalculator> logger) : IAutoCounterCalculator
{
    public async Task<decimal?> ComputeAsync(
        ActionField targetField,
        Guid trackedActionId,
        IReadOnlyDictionary<Guid, string?> currentValues,
        CancellationToken cancellationToken = default)
    {
        var config = ActionFieldMappingExtensions.DeserializeAutoCounter(targetField.AutoCounterConfigJson);
        if (config is null)
        {
            logger.LogDebug("AutoCounter[{Field}]: no config saved on the field; returning null.", targetField.Name);
            return null;
        }
        if (targetField.InitialValueBehavior != (int)InitialValueBehavior.AutoCounter)
        {
            logger.LogDebug("AutoCounter[{Field}]: behavior is not AutoCounter (was {Behavior}); returning null.",
                targetField.Name, targetField.InitialValueBehavior);
            return null;
        }

        if (config.Conditions is { Count: > 0 } && !ConditionsSatisfied(config, currentValues))
        {
            logger.LogDebug("AutoCounter[{Field}]: conditions reference fields not yet filled in current entry; returning null.",
                targetField.Name);
            return null;
        }

        var entries = await entryRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var sorted = entries
            .OrderByDescending(e => e.OccurredAtUtc)
            .ThenByDescending(e => e.CreatedAtUtc)
            .ToList();

        // Walk prior entries newest → oldest, returning the first one that (a) matches conditions
        // AND (b) has a parsable numeric value for the target field. Skipping entries with a blank
        // or unparseable target value means a stray empty entry doesn't reset the counter.
        foreach (var entry in sorted)
        {
            if (!MatchesAllConditions(entry, config, currentValues)) continue;

            var priorField = entry.Fields.FirstOrDefault(f => f.ActionFieldId == targetField.Id);
            var priorValueString = priorField?.Values
                .OrderBy(v => v.Order)
                .Select(v => v.Value)
                .FirstOrDefault();
            if (priorValueString is null) continue;

            if (!decimal.TryParse(priorValueString, NumberStyles.Any, CultureInfo.InvariantCulture, out var priorValue))
                continue;

            var next = priorValue + config.Step;
            logger.LogDebug(
                "AutoCounter[{Field}]: prior entry {EntryId} (occurred {OccurredAt:o}) had value '{PriorValue}', step {Step} → {Next}.",
                targetField.Name, entry.Id, entry.OccurredAtUtc, priorValue, config.Step, next);
            return next;
        }

        logger.LogDebug(
            "AutoCounter[{Field}]: no prior matching entry with parsable value among {Count} entries; returning StartValue={Start}.",
            targetField.Name, sorted.Count, config.StartValue);
        return config.StartValue;
    }

    private static bool ConditionsSatisfied(AutoCounterConfig config, IReadOnlyDictionary<Guid, string?> currentValues)
    {
        if (config.Conditions is null) return true;
        // For UseCurrentValue conditions, we need the in-progress entry to have filled that field
        // (otherwise we'd compare against an empty string and match every prior with empty values).
        // For literal-value conditions, the in-progress entry doesn't need that field at all.
        return config.Conditions.All(c =>
            !c.UseCurrentValue || currentValues.ContainsKey(c.FieldId));
    }

    private static bool MatchesAllConditions(
        ActionEntry entry,
        AutoCounterConfig config,
        IReadOnlyDictionary<Guid, string?> currentValues)
    {
        if (config.Conditions is not { Count: > 0 }) return true;

        bool MatchOne(AutoCounterCondition c)
        {
            var actual = entry.Fields
                .FirstOrDefault(f => f.ActionFieldId == c.FieldId)
                ?.Values
                .OrderBy(v => v.Order)
                .Select(v => v.Value)
                .FirstOrDefault();
            if (c.UseCurrentValue)
            {
                var expected = currentValues.GetValueOrDefault(c.FieldId);
                return EvaluateOperator(FilterOperator.Equals, actual, expected);
            }
            return EvaluateOperator(c.Operator, actual, c.Value);
        }

        return config.ConditionLogic == FilterLogic.Or
            ? config.Conditions.Any(MatchOne)
            : config.Conditions.All(MatchOne);
    }

    private static bool EvaluateOperator(FilterOperator op, string? actual, string? expected)
    {
        actual ??= "";
        expected ??= "";

        return op switch
        {
            FilterOperator.Equals => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            FilterOperator.NotEquals => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            FilterOperator.Contains => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            FilterOperator.NotContains => !actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            FilterOperator.StartsWith => actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase),
            FilterOperator.EndsWith => actual.EndsWith(expected, StringComparison.OrdinalIgnoreCase),
            FilterOperator.GreaterThan => CompareNumeric(actual, expected) > 0,
            FilterOperator.GreaterThanOrEqual => CompareNumeric(actual, expected) >= 0,
            FilterOperator.LessThan => CompareNumeric(actual, expected) < 0,
            FilterOperator.LessThanOrEqual => CompareNumeric(actual, expected) <= 0,
            FilterOperator.IsEmpty => string.IsNullOrEmpty(actual),
            FilterOperator.IsNotEmpty => !string.IsNullOrEmpty(actual),
            _ => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static int CompareNumeric(string a, string b)
    {
        if (decimal.TryParse(a, NumberStyles.Any, CultureInfo.InvariantCulture, out var ad)
            && decimal.TryParse(b, NumberStyles.Any, CultureInfo.InvariantCulture, out var bd))
        {
            return ad.CompareTo(bd);
        }
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
