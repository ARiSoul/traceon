using Microsoft.Extensions.Logging;
using System.Text.Json;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Contracts.ConnectedActionRules;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class ConnectedActionRuleService(
    IConnectedActionRuleRepository repository,
    ITrackedActionRepository actionRepository,
    ICurrentUserService currentUser,
    ILogger<ConnectedActionRuleService> logger) : IConnectedActionRuleService
{
    public async Task<Result<IReadOnlyList<ConnectedActionRuleResponse>>> GetBySourceTrackedActionIdAsync(
        Guid sourceTrackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(sourceTrackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(sourceTrackedActionId);
            return Result<IReadOnlyList<ConnectedActionRuleResponse>>.Failure(
                $"Tracked action with ID '{sourceTrackedActionId}' was not found.");
        }

        var sourceRules = await repository.GetBySourceTrackedActionIdAsync(sourceTrackedActionId, cancellationToken);
        var inboundRules = await repository.GetByTargetTrackedActionIdAsync(sourceTrackedActionId, cancellationToken);

        var allRules = sourceRules.Concat(inboundRules).ToList();

        var relatedActionIds = allRules
            .SelectMany(r => new[] { r.SourceTrackedActionId, r.TargetTrackedActionId })
            .Distinct()
            .ToList();

        var actionNames = new Dictionary<Guid, string>();
        foreach (var id in relatedActionIds)
        {
            if (actionNames.ContainsKey(id)) continue;
            if (id == sourceTrackedActionId)
            {
                actionNames[id] = action.Name;
                continue;
            }
            var related = await actionRepository.GetByIdAsync(id, cancellationToken);
            actionNames[id] = related?.Name ?? "?";
        }

        var responses = allRules
            .Select(r => r.ToResponse(
                actionNames.GetValueOrDefault(r.SourceTrackedActionId, "?"),
                actionNames.GetValueOrDefault(r.TargetTrackedActionId, "?")))
            .ToList();

        return Result<IReadOnlyList<ConnectedActionRuleResponse>>.Success(responses);
    }

    public async Task<Result<ConnectedActionRuleResponse>> CreateAsync(
        Guid sourceTrackedActionId, CreateConnectedActionRuleRequest request, CancellationToken cancellationToken = default)
    {
        var sourceAction = await actionRepository.GetByIdAsync(sourceTrackedActionId, cancellationToken);

        if (sourceAction is null || sourceAction.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(sourceTrackedActionId);
            return Result<ConnectedActionRuleResponse>.Failure(
                $"Tracked action with ID '{sourceTrackedActionId}' was not found.");
        }

        var targetAction = await actionRepository.GetByIdAsync(request.TargetTrackedActionId, cancellationToken);

        if (targetAction is null || targetAction.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(request.TargetTrackedActionId);
            return Result<ConnectedActionRuleResponse>.Failure(
                $"Target tracked action with ID '{request.TargetTrackedActionId}' was not found.");
        }

        if (sourceTrackedActionId == request.TargetTrackedActionId)
            return Result<ConnectedActionRuleResponse>.Failure(
                "Source and target actions must be different.", ResultErrorType.Validation);

        var entity = ConnectedActionRule.Create(
            sourceTrackedActionId,
            request.TargetTrackedActionId,
            request.Name,
            request.IsEnabled,
            request.ConditionsJson,
            request.FieldMappingsJson,
            request.CopyNotes,
            request.CopyDate,
            request.SortOrder);

        await repository.AddAsync(entity, cancellationToken);

        // Create reverse rule for bidirectional connections
        if (request.IsBidirectional)
        {
            var reverseName = $"{request.Name} ↩";
            var reverseMappingsJson = MergeReverseMappingsJson(
                BuildReverseMappingsJson(request.FieldMappingsJson),
                request.ReverseExtraMappingsJson);
            var reverseConditionsJson = BuildReverseConditionsJson(request.ConditionsJson, request.FieldMappingsJson);
            var reverseEntity = ConnectedActionRule.Create(
                request.TargetTrackedActionId,
                sourceTrackedActionId,
                reverseName,
                request.IsEnabled,
                reverseConditionsJson,
                reverseMappingsJson,
                copyNotes: request.CopyNotes,
                copyDate: request.CopyDate,
                pairedRuleId: entity.Id);

            await repository.AddAsync(reverseEntity, cancellationToken);

            // Link forward rule to reverse rule
            entity.SetPairedRuleId(reverseEntity.Id);
            await repository.UpdateAsync(entity, cancellationToken);
        }

        logger.ConnectedActionRuleCreated(entity.Id, sourceTrackedActionId);
        return Result<ConnectedActionRuleResponse>.Success(
            entity.ToResponse(sourceAction.Name, targetAction.Name));
    }

    public async Task<Result<ConnectedActionRuleResponse>> UpdateAsync(
        Guid id, UpdateConnectedActionRuleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ConnectedActionRuleNotFound(id);
            return Result<ConnectedActionRuleResponse>.Failure(
                $"Connected action rule with ID '{id}' was not found.");
        }

        entity.Update(
            request.Name,
            request.IsEnabled,
            request.ConditionsJson,
            request.FieldMappingsJson,
            request.CopyNotes,
            request.CopyDate,
            request.SortOrder,
            request.ClearConditions,
            request.ClearMappings);

        await repository.UpdateAsync(entity, cancellationToken);

        // Update reverse rule extra mappings if already bidirectional and extra mappings provided
        if (request.ReverseExtraMappingsJson is not null && entity.PairedRuleId.HasValue)
        {
            var pairedForUpdate = await repository.GetByIdAsync(entity.PairedRuleId.Value, cancellationToken);
            if (pairedForUpdate is not null)
            {
                var updatedReverseMappings = MergeReverseMappingsJson(
                    BuildReverseMappingsJson(entity.FieldMappingsJson),
                    request.ReverseExtraMappingsJson);
                pairedForUpdate.Update(
                    pairedForUpdate.Name,
                    pairedForUpdate.IsEnabled,
                    pairedForUpdate.ConditionsJson,
                    updatedReverseMappings,
                    pairedForUpdate.CopyNotes,
                    pairedForUpdate.CopyDate,
                    pairedForUpdate.SortOrder,
                    clearConditions: false,
                    clearMappings: false);
                await repository.UpdateAsync(pairedForUpdate, cancellationToken);
            }
        }

        // Handle bidirectional toggle changes
        if (request.IsBidirectional.HasValue)
        {
            if (request.IsBidirectional.Value && !entity.PairedRuleId.HasValue)
            {
                // Turning ON bidirectional — create the reverse rule
                var reverseName = $"{entity.Name} ↩";
                var reverseMappingsJson = MergeReverseMappingsJson(
                    BuildReverseMappingsJson(entity.FieldMappingsJson),
                    request.ReverseExtraMappingsJson);
                var reverseConditionsJson = BuildReverseConditionsJson(entity.ConditionsJson, entity.FieldMappingsJson);
                var reverseEntity = ConnectedActionRule.Create(
                    entity.TargetTrackedActionId,
                    entity.SourceTrackedActionId,
                    reverseName,
                    entity.IsEnabled,
                    reverseConditionsJson,
                    reverseMappingsJson,
                    copyNotes: entity.CopyNotes,
                    copyDate: entity.CopyDate,
                    pairedRuleId: entity.Id);

                await repository.AddAsync(reverseEntity, cancellationToken);

                entity.SetPairedRuleId(reverseEntity.Id);
                await repository.UpdateAsync(entity, cancellationToken);
            }
            else if (!request.IsBidirectional.Value && entity.PairedRuleId.HasValue)
            {
                // Turning OFF bidirectional — delete the reverse rule
                var paired = await repository.GetByIdAsync(entity.PairedRuleId.Value, cancellationToken);
                if (paired is not null)
                {
                    paired.ClearPairedRuleId();
                    await repository.UpdateAsync(paired, cancellationToken);
                    await repository.DeleteAsync(paired.Id, cancellationToken);
                }

                entity.ClearPairedRuleId();
                await repository.UpdateAsync(entity, cancellationToken);
            }
        }

        var sourceAction = await actionRepository.GetByIdAsync(entity.SourceTrackedActionId, cancellationToken);
        var targetAction = await actionRepository.GetByIdAsync(entity.TargetTrackedActionId, cancellationToken);

        logger.ConnectedActionRuleUpdated(id);
        return Result<ConnectedActionRuleResponse>.Success(
            entity.ToResponse(sourceAction?.Name ?? "?", targetAction?.Name ?? "?"));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ConnectedActionRuleNotFound(id);
            return Result.Failure($"Connected action rule with ID '{id}' was not found.");
        }

        // Delete paired (reverse) rule if it exists
        if (entity.PairedRuleId.HasValue)
        {
            var paired = await repository.GetByIdAsync(entity.PairedRuleId.Value, cancellationToken);
            if (paired is not null)
            {
                // Clear the paired reference before deleting to avoid circular issues
                paired.ClearPairedRuleId();
                await repository.UpdateAsync(paired, cancellationToken);
                await repository.DeleteAsync(paired.Id, cancellationToken);
            }
        }

        await repository.DeleteAsync(id, cancellationToken);

        logger.ConnectedActionRuleDeleted(id);
        return Result.Success();
    }

    private static string? BuildReverseMappingsJson(string? mappingsJson)
    {
        var forwardMappings = DeserializeMappings(mappingsJson);
        if (forwardMappings.Count == 0) return null;

        var reverseMappings = forwardMappings
            .Select(TryReverseMapping)
            .Where(m => m is not null)
            .Select(m => m!)
            .ToList();

        return reverseMappings.Count == 0
            ? null
            : JsonSerializer.Serialize(reverseMappings, JsonOpts);
    }

    private static string? MergeReverseMappingsJson(string? reversedMappingsJson, string? extraMappingsJson)
    {
        var reversed = DeserializeMappings(reversedMappingsJson);
        var extras = DeserializeMappings(extraMappingsJson);

        if (extras.Count == 0) return reversedMappingsJson;

        // Only add extras for target fields not already covered by reversed mappings
        var coveredTargetIds = reversed.Select(m => m.TargetFieldId).ToHashSet();
        foreach (var extra in extras)
        {
            if (extra.TargetFieldId != Guid.Empty && !coveredTargetIds.Contains(extra.TargetFieldId))
            {
                reversed.Add(extra);
            }
        }

        return reversed.Count == 0
            ? null
            : JsonSerializer.Serialize(reversed, JsonOpts);
    }

    private static string? BuildReverseConditionsJson(string? conditionsJson, string? mappingsJson)
    {
        var forwardConditions = DeserializeConditions(conditionsJson);
        if (forwardConditions.Count == 0) return null;

        var forwardMappings = DeserializeMappings(mappingsJson);

        var reverseConditions = new List<RuleConditionDto>();
        foreach (var cond in forwardConditions)
        {
            if (cond.SourceFieldId == Guid.Empty) continue;

            var mapped = forwardMappings.FirstOrDefault(m => m.SourceFieldId == cond.SourceFieldId);
            if (mapped is null || mapped.TargetFieldId == Guid.Empty)
                continue;

            var reversedValue = cond.Value;
            if (mapped.Mode.Equals("Conditional", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(cond.Value)
                && mapped.ValueMappings is { Count: > 0 })
            {
                var valueMap = mapped.ValueMappings.FirstOrDefault(v =>
                    string.Equals(v.When, cond.Value, StringComparison.OrdinalIgnoreCase));
                if (valueMap is not null && !string.IsNullOrWhiteSpace(valueMap.Then))
                    reversedValue = valueMap.Then;
            }

            reverseConditions.Add(new RuleConditionDto
            {
                SourceFieldId = mapped.TargetFieldId,
                Operator = string.IsNullOrWhiteSpace(cond.Operator) ? "Equals" : cond.Operator,
                Value = reversedValue,
                Value2 = cond.Value2
            });
        }

        return reverseConditions.Count == 0
            ? null
            : JsonSerializer.Serialize(reverseConditions, JsonOpts);
    }

    private static RuleMappingDto? TryReverseMapping(RuleMappingDto mapping)
    {
        if (mapping.TargetFieldId == Guid.Empty || mapping.SourceFieldId == Guid.Empty)
            return null;

        var mode = string.IsNullOrWhiteSpace(mapping.Mode) ? "Source" : mapping.Mode;
        if (mode.Equals("Static", StringComparison.OrdinalIgnoreCase))
            return null;

        if (mode.Equals("Conditional", StringComparison.OrdinalIgnoreCase))
        {
            var reversePairs = mapping.ValueMappings?
                .Where(v => !string.IsNullOrWhiteSpace(v.When) || !string.IsNullOrWhiteSpace(v.Then))
                .Select(v => new RuleValueMapDto
                {
                    When = v.Then ?? string.Empty,
                    Then = v.When ?? string.Empty
                })
                .ToList() ?? [];

            return new RuleMappingDto
            {
                TargetFieldId = mapping.SourceFieldId,
                Mode = "Conditional",
                SourceFieldId = mapping.TargetFieldId,
                ValueMappings = reversePairs
            };
        }

        return new RuleMappingDto
        {
            TargetFieldId = mapping.SourceFieldId,
            Mode = "Source",
            SourceFieldId = mapping.TargetFieldId
        };
    }

    private static List<RuleMappingDto> DeserializeMappings(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];

        try
        {
            return JsonSerializer.Deserialize<List<RuleMappingDto>>(json, JsonOpts) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static List<RuleConditionDto> DeserializeConditions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];

        try
        {
            return JsonSerializer.Deserialize<List<RuleConditionDto>>(json, JsonOpts) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private sealed class RuleConditionDto
    {
        public Guid SourceFieldId { get; set; }
        public string Operator { get; set; } = "Equals";
        public string Value { get; set; } = string.Empty;
        public string? Value2 { get; set; }
    }

    private sealed class RuleMappingDto
    {
        public Guid TargetFieldId { get; set; }
        public string Mode { get; set; } = "Source";
        public Guid SourceFieldId { get; set; }
        public string? StaticValue { get; set; }
        public List<RuleValueMapDto>? ValueMappings { get; set; }
    }

    private sealed class RuleValueMapDto
    {
        public string? When { get; set; }
        public string? Then { get; set; }
    }
}
