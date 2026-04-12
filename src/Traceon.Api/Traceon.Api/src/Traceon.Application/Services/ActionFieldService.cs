using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Contracts.ActionFields;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class ActionFieldService(
    IActionFieldRepository repository,
    ITrackedActionRepository actionRepository,
    IFieldDefinitionRepository fieldDefinitionRepository,
    ICurrentUserService currentUser,
    ILogger<ActionFieldService> logger) : IActionFieldService
{
    public async Task<Result<IQueryable<ActionFieldResponse>>> QueryByTrackedActionIdAsync(
        Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<IQueryable<ActionFieldResponse>>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var queryable = from af in repository.Query()
                        where af.TrackedActionId == trackedActionId
                        join fd in fieldDefinitionRepository.Query()
                            on af.FieldDefinitionId equals fd.Id
                        orderby af.Order
                        select new ActionFieldResponse
                        {
                            CreatedAtUtc = af.CreatedAtUtc,
                            DefaultValue = af.DefaultValue,
                            Description = af.Description,
                            FieldDefinitionName = fd.DefaultName,
                            FieldDefinitionId = fd.Id,
                            FieldType = fd.Type,
                            Id = af.Id,
                            IsRequired = af.IsRequired,
                            MaxValue = af.MaxValue,
                            MinValue = af.MinValue,
                            Name = af.Name,
                            Order = af.Order,
                            SummaryMetrics = (Contracts.Enums.SummaryMetrics)af.SummaryMetrics,
                            TrendAggregation = (Contracts.Enums.TrendAggregation)af.TrendAggregation,
                            TrendChartType = (Contracts.Enums.TrendChartType)af.TrendChartType,
                            TargetValue = af.TargetValue,
                            TrackedActionId = trackedActionId,
                            UpdatedAtUtc = af.UpdatedAtUtc,
                            Unit = af.Unit,
                            DropdownValues = fd.DropdownValues,
                            InitialValueBehavior = (Contracts.Enums.InitialValueBehavior)af.InitialValueBehavior,
                            InitialValuePeriodUnit = (Contracts.Enums.InitialValuePeriodUnit)af.InitialValuePeriodUnit,
                            InitialValuePeriodCount = af.InitialValuePeriodCount,
                            DropdownTrendValueFieldId = af.DropdownTrendValueFieldId,
                            DropdownTrendAggregation = (Contracts.Enums.TrendAggregation)af.DropdownTrendAggregation,
                            DropdownTrendChartType = (Contracts.Enums.TrendChartType)af.DropdownTrendChartType
                        };

        return Result<IQueryable<ActionFieldResponse>>.Success(queryable);
    }

    public async Task<Result<ActionFieldResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ActionFieldNotFound(id);
            return Result<ActionFieldResponse>.Failure($"Action field with ID '{id}' was not found.");
        }

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(entity.FieldDefinitionId, cancellationToken);

        if (fieldDef is null)
        {
            logger.FieldDefinitionNotFound(entity.FieldDefinitionId);
            return Result<ActionFieldResponse>.Failure($"Field definition with ID '{entity.FieldDefinitionId}' was not found.");
        }

        return Result<ActionFieldResponse>.Success(entity.ToResponse(fieldDef));
    }

    public async Task<Result<IReadOnlyList<ActionFieldResponse>>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<IReadOnlyList<ActionFieldResponse>>.Failure($"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var fields = await repository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldDefs = await fieldDefinitionRepository.GetAllByUserIdAsync(currentUser.UserId, cancellationToken);
        var fieldDefMap = fieldDefs.ToDictionary(fd => fd.Id);

        var responses = fields
            .Where(f => fieldDefMap.ContainsKey(f.FieldDefinitionId))
            .Select(f => f.ToResponse(fieldDefMap[f.FieldDefinitionId]))
            .ToList();

        return Result<IReadOnlyList<ActionFieldResponse>>.Success(responses);
    }

    public async Task<Result<ActionFieldResponse>> CreateAsync(Guid trackedActionId, CreateActionFieldRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<ActionFieldResponse>.Failure($"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(request.FieldDefinitionId, cancellationToken);

        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
        {
            logger.FieldDefinitionNotFound(request.FieldDefinitionId);
            return Result<ActionFieldResponse>.Failure($"Field definition with ID '{request.FieldDefinitionId}' was not found.");
        }

        var entity = ActionField.Create(
            trackedActionId,
            request.FieldDefinitionId,
            request.Name,
            request.Description,
            request.MaxValue,
            request.MinValue,
            request.IsRequired,
            request.DefaultValue,
            request.Unit,
            request.Order,
            (int)request.SummaryMetrics,
            (int)request.TrendAggregation,
            (int)request.TrendChartType,
            request.TargetValue,
            (int)request.InitialValueBehavior,
            (int)request.InitialValuePeriodUnit,
            request.InitialValuePeriodCount,
            request.DropdownTrendValueFieldId,
            (int)request.DropdownTrendAggregation,
            (int)request.DropdownTrendChartType);

        await repository.AddAsync(entity, cancellationToken);

        logger.ActionFieldCreated(entity.Name, entity.Id, trackedActionId);
        return Result<ActionFieldResponse>.Success(entity.ToResponse(fieldDef));
    }

    public async Task<Result<ActionFieldResponse>> UpdateAsync(Guid id, UpdateActionFieldRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ActionFieldNotFound(id);
            return Result<ActionFieldResponse>.Failure($"Action field with ID '{id}' was not found.");
        }

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(entity.FieldDefinitionId, cancellationToken);

        if (fieldDef is null)
        {
            logger.FieldDefinitionNotFound(entity.FieldDefinitionId);
            return Result<ActionFieldResponse>.Failure($"Field definition with ID '{entity.FieldDefinitionId}' was not found.");
        }

        entity.Update(
            request.Name,
            request.Description,
            request.MaxValue,
            request.MinValue,
            request.IsRequired,
            request.DefaultValue,
            request.Unit,
            request.Order,
            request.SummaryMetrics.HasValue ? (int)request.SummaryMetrics.Value : null,
            request.TrendAggregation.HasValue ? (int)request.TrendAggregation.Value : null,
            request.TrendChartType.HasValue ? (int)request.TrendChartType.Value : null,
            request.TargetValue,
            request.InitialValueBehavior.HasValue ? (int)request.InitialValueBehavior.Value : null,
            request.InitialValuePeriodUnit.HasValue ? (int)request.InitialValuePeriodUnit.Value : null,
            request.InitialValuePeriodCount,
            request.DropdownTrendValueFieldId,
            request.DropdownTrendAggregation.HasValue ? (int)request.DropdownTrendAggregation.Value : null,
            request.DropdownTrendChartType.HasValue ? (int)request.DropdownTrendChartType.Value : null);

        await repository.UpdateAsync(entity, cancellationToken);

        logger.ActionFieldUpdated(id);
        return Result<ActionFieldResponse>.Success(entity.ToResponse(fieldDef));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ActionFieldNotFound(id);
            return Result.Failure($"Action field with ID '{id}' was not found.");
        }

        await repository.DeleteAsync(id, cancellationToken);

        logger.ActionFieldDeleted(id);
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetDeletedByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ActionFieldNotFound(id);
            return Result.Failure($"Deleted action field with ID '{id}' was not found.");
        }

        var action = await actionRepository.GetByIdAsync(entity.TrackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.ActionFieldNotFound(id);
            return Result.Failure($"Deleted action field with ID '{id}' was not found.");
        }

        await repository.RestoreAsync(id, cancellationToken);

        logger.ActionFieldRestored(id);
        return Result.Success();
    }
}
