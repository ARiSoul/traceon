using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Contracts.FieldDependencyRules;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class FieldDependencyRuleService(
    IFieldDependencyRuleRepository repository,
    ITrackedActionRepository actionRepository,
    IActionFieldRepository fieldRepository,
    ICurrentUserService currentUser,
    ILogger<FieldDependencyRuleService> logger) : IFieldDependencyRuleService
{
    public async Task<Result<IReadOnlyList<FieldDependencyRuleResponse>>> GetByTrackedActionIdAsync(
        Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<IReadOnlyList<FieldDependencyRuleResponse>>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var rules = await repository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        var responses = rules
            .Select(r => r.ToResponse(
                fieldNames.GetValueOrDefault(r.SourceFieldId, "?"),
                fieldNames.GetValueOrDefault(r.TargetFieldId, "?")))
            .ToList();

        return Result<IReadOnlyList<FieldDependencyRuleResponse>>.Success(responses);
    }

    public async Task<Result<FieldDependencyRuleResponse>> CreateAsync(
        Guid trackedActionId, CreateFieldDependencyRuleRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<FieldDependencyRuleResponse>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldIds = fields.Select(f => f.Id).ToHashSet();

        if (!fieldIds.Contains(request.SourceFieldId))
            return Result<FieldDependencyRuleResponse>.Failure(
                $"Source field '{request.SourceFieldId}' not found in this action.", ResultErrorType.Validation);

        if (!fieldIds.Contains(request.TargetFieldId))
            return Result<FieldDependencyRuleResponse>.Failure(
                $"Target field '{request.TargetFieldId}' not found in this action.", ResultErrorType.Validation);

        var entity = FieldDependencyRule.Create(
            trackedActionId,
            request.SourceFieldId,
            request.TargetFieldId,
            request.SourceValue,
            (int)request.RuleType,
            request.TargetConstraint,
            request.SortOrder);

        await repository.AddAsync(entity, cancellationToken);

        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        logger.FieldDependencyRuleCreated(entity.Id, trackedActionId);
        return Result<FieldDependencyRuleResponse>.Success(entity.ToResponse(
            fieldNames.GetValueOrDefault(entity.SourceFieldId, "?"),
            fieldNames.GetValueOrDefault(entity.TargetFieldId, "?")));
    }

    public async Task<Result<FieldDependencyRuleResponse>> UpdateAsync(
        Guid id, UpdateFieldDependencyRuleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.FieldDependencyRuleNotFound(id);
            return Result<FieldDependencyRuleResponse>.Failure(
                $"Dependency rule with ID '{id}' was not found.");
        }

        entity.Update(
            request.SourceValue,
            request.RuleType.HasValue ? (int)request.RuleType.Value : null,
            request.TargetConstraint,
            request.SortOrder);

        await repository.UpdateAsync(entity, cancellationToken);

        var fields = await fieldRepository.GetByTrackedActionIdAsync(entity.TrackedActionId, cancellationToken);
        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        logger.FieldDependencyRuleUpdated(id);
        return Result<FieldDependencyRuleResponse>.Success(entity.ToResponse(
            fieldNames.GetValueOrDefault(entity.SourceFieldId, "?"),
            fieldNames.GetValueOrDefault(entity.TargetFieldId, "?")));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.FieldDependencyRuleNotFound(id);
            return Result.Failure($"Dependency rule with ID '{id}' was not found.");
        }

        await repository.DeleteAsync(id, cancellationToken);

        logger.FieldDependencyRuleDeleted(id);
        return Result.Success();
    }
}
