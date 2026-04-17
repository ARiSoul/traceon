using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Contracts.Enums;
using Traceon.Contracts.FieldAnalyticsRules;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class FieldAnalyticsRuleService(
    IFieldAnalyticsRuleRepository repository,
    ITrackedActionRepository actionRepository,
    IActionFieldRepository fieldRepository,
    IDropdownValueMetadataFieldRepository metadataFieldRepository,
    IActionChartVisibilityService chartVisibilityService,
    ICurrentUserService currentUser,
    ILogger<FieldAnalyticsRuleService> logger) : IFieldAnalyticsRuleService
{
    public async Task<Result<IReadOnlyList<FieldAnalyticsRuleResponse>>> GetByTrackedActionIdAsync(
        Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<IReadOnlyList<FieldAnalyticsRuleResponse>>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var rules = await repository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        var metadataIds = rules
            .SelectMany(r => new[] { r.GroupByMetadataFieldId, r.FilterMetadataFieldId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var metadataNames = await LoadMetadataFieldNamesAsync(metadataIds, cancellationToken);

        var responses = rules
            .Select(r => r.ToResponse(
                fieldNames.GetValueOrDefault(r.MeasureFieldId, "?"),
                fieldNames.GetValueOrDefault(r.GroupByFieldId, "?"),
                r.FilterFieldId.HasValue ? fieldNames.GetValueOrDefault(r.FilterFieldId.Value, "?") : null,
                r.SignFieldId.HasValue ? fieldNames.GetValueOrDefault(r.SignFieldId.Value, "?") : null,
                r.GroupByMetadataFieldId.HasValue ? metadataNames.GetValueOrDefault(r.GroupByMetadataFieldId.Value) : null,
                r.FilterMetadataFieldId.HasValue ? metadataNames.GetValueOrDefault(r.FilterMetadataFieldId.Value) : null))
            .ToList();

        return Result<IReadOnlyList<FieldAnalyticsRuleResponse>>.Success(responses);
    }

    public async Task<Result<FieldAnalyticsRuleResponse>> CreateAsync(
        Guid trackedActionId, CreateFieldAnalyticsRuleRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldsById = fields.ToDictionary(f => f.Id);

        if (!fieldsById.ContainsKey(request.MeasureFieldId))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Measure field '{request.MeasureFieldId}' not found in this action.", ResultErrorType.Validation);

        if (!fieldsById.ContainsKey(request.GroupByFieldId))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Group-by field '{request.GroupByFieldId}' not found in this action.", ResultErrorType.Validation);

        if (request.FilterFieldId.HasValue && !fieldsById.ContainsKey(request.FilterFieldId.Value))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Filter field '{request.FilterFieldId}' not found in this action.", ResultErrorType.Validation);

        if (request.SignFieldId.HasValue && !fieldsById.ContainsKey(request.SignFieldId.Value))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Sign field '{request.SignFieldId}' not found in this action.", ResultErrorType.Validation);

        var metadataValidation = await ValidateMetadataLinksAsync(
            fieldsById,
            request.GroupByFieldId,
            request.GroupByMetadataFieldId,
            request.FilterFieldId,
            request.FilterMetadataFieldId,
            cancellationToken);
        if (metadataValidation is not null)
            return Result<FieldAnalyticsRuleResponse>.Failure(metadataValidation, ResultErrorType.Validation);

        var entity = FieldAnalyticsRule.Create(
            trackedActionId,
            request.MeasureFieldId,
            request.GroupByFieldId,
            (int)request.Aggregation,
            (int)request.DisplayType,
            request.FilterFieldId,
            request.FilterValue,
            request.Label,
            request.SortOrder,
            request.SignFieldId,
            request.NegativeValues,
            request.GroupByMetadataFieldId,
            request.FilterMetadataFieldId);

        await repository.AddAsync(entity, cancellationToken);

        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);
        var metadataNames = await LoadMetadataFieldNamesAsync(
            new[] { entity.GroupByMetadataFieldId, entity.FilterMetadataFieldId }
                .Where(id => id.HasValue).Select(id => id!.Value).ToList(),
            cancellationToken);

        logger.FieldAnalyticsRuleCreated(entity.Id, trackedActionId);
        return Result<FieldAnalyticsRuleResponse>.Success(entity.ToResponse(
            fieldNames.GetValueOrDefault(entity.MeasureFieldId, "?"),
            fieldNames.GetValueOrDefault(entity.GroupByFieldId, "?"),
            entity.FilterFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.FilterFieldId.Value, "?") : null,
            entity.SignFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.SignFieldId.Value, "?") : null,
            entity.GroupByMetadataFieldId.HasValue ? metadataNames.GetValueOrDefault(entity.GroupByMetadataFieldId.Value) : null,
            entity.FilterMetadataFieldId.HasValue ? metadataNames.GetValueOrDefault(entity.FilterMetadataFieldId.Value) : null));
    }

    public async Task<Result<FieldAnalyticsRuleResponse>> UpdateAsync(
        Guid id, UpdateFieldAnalyticsRuleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.FieldAnalyticsRuleNotFound(id);
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Analytics rule with ID '{id}' was not found.");
        }

        var fields = await fieldRepository.GetByTrackedActionIdAsync(entity.TrackedActionId, cancellationToken);
        var fieldsById = fields.ToDictionary(f => f.Id);

        if (request.FilterFieldId.HasValue && !fieldsById.ContainsKey(request.FilterFieldId.Value))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Filter field '{request.FilterFieldId}' not found in this action.", ResultErrorType.Validation);

        if (request.SignFieldId.HasValue && !fieldsById.ContainsKey(request.SignFieldId.Value))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Sign field '{request.SignFieldId}' not found in this action.", ResultErrorType.Validation);

        var metadataValidation = await ValidateMetadataLinksAsync(
            fieldsById,
            entity.GroupByFieldId,
            request.ClearGroupByMetadataField ? null : (request.GroupByMetadataFieldId ?? entity.GroupByMetadataFieldId),
            request.FilterFieldId,
            request.ClearFilterMetadataField ? null : (request.FilterMetadataFieldId ?? entity.FilterMetadataFieldId),
            cancellationToken);
        if (metadataValidation is not null)
            return Result<FieldAnalyticsRuleResponse>.Failure(metadataValidation, ResultErrorType.Validation);

        var oldAggregation = entity.Aggregation;
        var oldLabel = entity.Label;

        entity.Update(
            request.Aggregation.HasValue ? (int)request.Aggregation.Value : null,
            request.DisplayType.HasValue ? (int)request.DisplayType.Value : null,
            request.FilterFieldId,
            request.FilterValue,
            request.Label,
            request.SortOrder,
            request.SignFieldId,
            request.NegativeValues,
            request.ClearSignField,
            request.GroupByMetadataFieldId,
            request.ClearGroupByMetadataField,
            request.FilterMetadataFieldId,
            request.ClearFilterMetadataField);

        await repository.UpdateAsync(entity, cancellationToken);

        await MigrateBalanceChartKeyIfChangedAsync(entity, oldAggregation, oldLabel, fieldsById, cancellationToken);

        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);
        var metadataNames = await LoadMetadataFieldNamesAsync(
            new[] { entity.GroupByMetadataFieldId, entity.FilterMetadataFieldId }
                .Where(mid => mid.HasValue).Select(mid => mid!.Value).ToList(),
            cancellationToken);

        logger.FieldAnalyticsRuleUpdated(id);
        return Result<FieldAnalyticsRuleResponse>.Success(entity.ToResponse(
            fieldNames.GetValueOrDefault(entity.MeasureFieldId, "?"),
            fieldNames.GetValueOrDefault(entity.GroupByFieldId, "?"),
            entity.FilterFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.FilterFieldId.Value, "?") : null,
            entity.SignFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.SignFieldId.Value, "?") : null,
            entity.GroupByMetadataFieldId.HasValue ? metadataNames.GetValueOrDefault(entity.GroupByMetadataFieldId.Value) : null,
            entity.FilterMetadataFieldId.HasValue ? metadataNames.GetValueOrDefault(entity.FilterMetadataFieldId.Value) : null));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.FieldAnalyticsRuleNotFound(id);
            return Result.Failure($"Analytics rule with ID '{id}' was not found.");
        }

        await repository.DeleteAsync(id, cancellationToken);

        logger.FieldAnalyticsRuleDeleted(id);
        return Result.Success();
    }

    private async Task MigrateBalanceChartKeyIfChangedAsync(
        FieldAnalyticsRule entity,
        int oldAggregation,
        string? oldLabel,
        Dictionary<Guid, ActionField> fieldsById,
        CancellationToken cancellationToken)
    {
        var signedSum = (int)AnalyticsAggregation.SignedSum;
        if (oldAggregation != signedSum || entity.Aggregation != signedSum)
            return;

        if (!fieldsById.TryGetValue(entity.MeasureFieldId, out var measureField))
            return;

        var oldKeyLabel = string.IsNullOrWhiteSpace(oldLabel) ? measureField.Name : oldLabel!;
        var newKeyLabel = string.IsNullOrWhiteSpace(entity.Label) ? measureField.Name : entity.Label!;
        if (string.Equals(oldKeyLabel, newKeyLabel, StringComparison.Ordinal))
            return;

        var renames = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [$"bal-{oldKeyLabel}"] = $"bal-{newKeyLabel}",
        };

        await chartVisibilityService.RenameKeysAsync(entity.TrackedActionId, renames, cancellationToken);
    }

    private async Task<string?> ValidateMetadataLinksAsync(
        Dictionary<Guid, ActionField> fieldsById,
        Guid groupByFieldId,
        Guid? groupByMetadataFieldId,
        Guid? filterFieldId,
        Guid? filterMetadataFieldId,
        CancellationToken cancellationToken)
    {
        if (groupByMetadataFieldId.HasValue)
        {
            var error = await ValidateMetadataBelongsToAsync(
                fieldsById[groupByFieldId].FieldDefinitionId,
                groupByMetadataFieldId.Value,
                "group-by",
                cancellationToken);
            if (error is not null) return error;
        }

        if (filterMetadataFieldId.HasValue)
        {
            if (!filterFieldId.HasValue)
                return "Filter field must be set when a filter metadata field is specified.";

            var error = await ValidateMetadataBelongsToAsync(
                fieldsById[filterFieldId.Value].FieldDefinitionId,
                filterMetadataFieldId.Value,
                "filter",
                cancellationToken);
            if (error is not null) return error;
        }

        return null;
    }

    private async Task<string?> ValidateMetadataBelongsToAsync(
        Guid fieldDefinitionId,
        Guid metadataFieldId,
        string role,
        CancellationToken cancellationToken)
    {
        var metaField = await metadataFieldRepository.GetByIdAsync(metadataFieldId, cancellationToken);
        if (metaField is null || metaField.FieldDefinitionId != fieldDefinitionId)
            return $"The {role} metadata field '{metadataFieldId}' does not belong to the selected {role} field's dropdown definition.";
        return null;
    }

    private async Task<Dictionary<Guid, string>> LoadMetadataFieldNamesAsync(
        List<Guid> ids, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, string>();
        foreach (var id in ids)
        {
            var field = await metadataFieldRepository.GetByIdAsync(id, cancellationToken);
            if (field is not null) result[id] = field.Name;
        }
        return result;
    }
}
