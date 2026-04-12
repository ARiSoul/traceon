using Microsoft.Extensions.Logging;
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

        var rules = await repository.GetBySourceTrackedActionIdAsync(sourceTrackedActionId, cancellationToken);

        var targetIds = rules.Select(r => r.TargetTrackedActionId).Distinct().ToList();
        var actionNames = new Dictionary<Guid, string> { [sourceTrackedActionId] = action.Name };

        foreach (var targetId in targetIds)
        {
            if (actionNames.ContainsKey(targetId)) continue;
            var target = await actionRepository.GetByIdAsync(targetId, cancellationToken);
            actionNames[targetId] = target?.Name ?? "?";
        }

        var responses = rules
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

        await repository.DeleteAsync(id, cancellationToken);

        logger.ConnectedActionRuleDeleted(id);
        return Result.Success();
    }
}
