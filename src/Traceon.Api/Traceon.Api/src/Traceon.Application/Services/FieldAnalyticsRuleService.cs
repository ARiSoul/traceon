using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Contracts.FieldAnalyticsRules;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class FieldAnalyticsRuleService(
    IFieldAnalyticsRuleRepository repository,
    ITrackedActionRepository actionRepository,
    IActionFieldRepository fieldRepository,
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

        var responses = rules
            .Select(r => r.ToResponse(
                fieldNames.GetValueOrDefault(r.MeasureFieldId, "?"),
                fieldNames.GetValueOrDefault(r.GroupByFieldId, "?"),
                r.FilterFieldId.HasValue ? fieldNames.GetValueOrDefault(r.FilterFieldId.Value, "?") : null,
                r.SignFieldId.HasValue ? fieldNames.GetValueOrDefault(r.SignFieldId.Value, "?") : null))
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
        var fieldIds = fields.Select(f => f.Id).ToHashSet();

        if (!fieldIds.Contains(request.MeasureFieldId))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Measure field '{request.MeasureFieldId}' not found in this action.", ResultErrorType.Validation);

        if (!fieldIds.Contains(request.GroupByFieldId))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Group-by field '{request.GroupByFieldId}' not found in this action.", ResultErrorType.Validation);

        if (request.FilterFieldId.HasValue && !fieldIds.Contains(request.FilterFieldId.Value))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Filter field '{request.FilterFieldId}' not found in this action.", ResultErrorType.Validation);

        if (request.SignFieldId.HasValue && !fieldIds.Contains(request.SignFieldId.Value))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Sign field '{request.SignFieldId}' not found in this action.", ResultErrorType.Validation);

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
            request.NegativeValues);

        await repository.AddAsync(entity, cancellationToken);

        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        logger.FieldAnalyticsRuleCreated(entity.Id, trackedActionId);
        return Result<FieldAnalyticsRuleResponse>.Success(entity.ToResponse(
            fieldNames.GetValueOrDefault(entity.MeasureFieldId, "?"),
            fieldNames.GetValueOrDefault(entity.GroupByFieldId, "?"),
            entity.FilterFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.FilterFieldId.Value, "?") : null,
            entity.SignFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.SignFieldId.Value, "?") : null));
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
        var fieldIds = fields.Select(f => f.Id).ToHashSet();

        if (request.FilterFieldId.HasValue && !fieldIds.Contains(request.FilterFieldId.Value))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Filter field '{request.FilterFieldId}' not found in this action.", ResultErrorType.Validation);

        if (request.SignFieldId.HasValue && !fieldIds.Contains(request.SignFieldId.Value))
            return Result<FieldAnalyticsRuleResponse>.Failure(
                $"Sign field '{request.SignFieldId}' not found in this action.", ResultErrorType.Validation);

        entity.Update(
            request.Aggregation.HasValue ? (int)request.Aggregation.Value : null,
            request.DisplayType.HasValue ? (int)request.DisplayType.Value : null,
            request.FilterFieldId,
            request.FilterValue,
            request.Label,
            request.SortOrder,
            request.SignFieldId,
            request.NegativeValues,
            request.ClearSignField);

        await repository.UpdateAsync(entity, cancellationToken);

        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        logger.FieldAnalyticsRuleUpdated(id);
        return Result<FieldAnalyticsRuleResponse>.Success(entity.ToResponse(
            fieldNames.GetValueOrDefault(entity.MeasureFieldId, "?"),
            fieldNames.GetValueOrDefault(entity.GroupByFieldId, "?"),
            entity.FilterFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.FilterFieldId.Value, "?") : null,
            entity.SignFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.SignFieldId.Value, "?") : null));
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
}
