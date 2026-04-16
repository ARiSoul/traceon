using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Contracts.CustomCharts;
using Traceon.Contracts.Enums;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class CustomChartService(
    ICustomChartRepository repository,
    ITrackedActionRepository actionRepository,
    IActionFieldRepository fieldRepository,
    ICurrentUserService currentUser,
    ILogger<CustomChartService> logger) : ICustomChartService
{
    public async Task<Result<IReadOnlyList<CustomChartResponse>>> GetByTrackedActionIdAsync(
        Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<IReadOnlyList<CustomChartResponse>>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var charts = await repository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        var responses = charts
            .Select(c => c.ToResponse(
                fieldNames.GetValueOrDefault(c.MeasureFieldId, "?"),
                c.GroupByFieldId.HasValue ? fieldNames.GetValueOrDefault(c.GroupByFieldId.Value, "?") : null))
            .ToList();

        return Result<IReadOnlyList<CustomChartResponse>>.Success(responses);
    }

    public async Task<Result<CustomChartResponse>> CreateAsync(
        Guid trackedActionId, CreateCustomChartRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<CustomChartResponse>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldsById = fields.ToDictionary(f => f.Id);

        if (!fieldsById.ContainsKey(request.MeasureFieldId))
            return Result<CustomChartResponse>.Failure(
                $"Measure field '{request.MeasureFieldId}' not found in this action.", ResultErrorType.Validation);

        if (request.GroupByFieldId.HasValue && !fieldsById.ContainsKey(request.GroupByFieldId.Value))
            return Result<CustomChartResponse>.Failure(
                $"Group-by field '{request.GroupByFieldId}' not found in this action.", ResultErrorType.Validation);

        var validFieldIds = fieldsById.Keys.ToHashSet();
        var filterError = ValidateFilterTree(request.FilterConditions, validFieldIds);
        if (filterError is not null)
            return Result<CustomChartResponse>.Failure(filterError, ResultErrorType.Validation);

        var filterJson = CustomChartMappingExtensions.SerializeFilterConditions(request.FilterConditions);

        var entity = CustomChart.Create(
            trackedActionId,
            request.Title,
            request.MeasureFieldId,
            (int)request.Aggregation,
            (int)request.ChartType,
            request.GroupByFieldId,
            (int)request.TimeGrouping,
            filterJson,
            request.ColorPalette,
            request.SortOrder,
            request.SortDescending,
            request.MaxGroups);

        await repository.AddAsync(entity, cancellationToken);

        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        logger.CustomChartCreated(entity.Id, trackedActionId);
        return Result<CustomChartResponse>.Success(entity.ToResponse(
            fieldNames.GetValueOrDefault(entity.MeasureFieldId, "?"),
            entity.GroupByFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.GroupByFieldId.Value, "?") : null));
    }

    public async Task<Result<CustomChartResponse>> UpdateAsync(
        Guid id, UpdateCustomChartRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.CustomChartNotFound(id);
            return Result<CustomChartResponse>.Failure(
                $"Custom chart with ID '{id}' was not found.");
        }

        var action = await actionRepository.GetByIdAsync(entity.TrackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(entity.TrackedActionId);
            return Result<CustomChartResponse>.Failure(
                $"Tracked action with ID '{entity.TrackedActionId}' was not found.");
        }

        var fields = await fieldRepository.GetByTrackedActionIdAsync(entity.TrackedActionId, cancellationToken);
        var fieldsById = fields.ToDictionary(f => f.Id);

        if (request.GroupByFieldId.HasValue && !fieldsById.ContainsKey(request.GroupByFieldId.Value))
            return Result<CustomChartResponse>.Failure(
                $"Group-by field '{request.GroupByFieldId}' not found in this action.", ResultErrorType.Validation);

        string? filterJson = null;
        bool clearFilter = request.ClearFilterConditions;
        if (request.FilterConditions is not null)
        {
            var validFieldIds = fieldsById.Keys.ToHashSet();
            var filterError = ValidateFilterTree(request.FilterConditions, validFieldIds);
            if (filterError is not null)
                return Result<CustomChartResponse>.Failure(filterError, ResultErrorType.Validation);

            filterJson = CustomChartMappingExtensions.SerializeFilterConditions(request.FilterConditions);
        }

        entity.Update(
            request.Title,
            request.Aggregation.HasValue ? (int)request.Aggregation.Value : null,
            request.ChartType.HasValue ? (int)request.ChartType.Value : null,
            request.GroupByFieldId,
            request.ClearGroupByField,
            request.TimeGrouping.HasValue ? (int)request.TimeGrouping.Value : null,
            filterJson,
            clearFilter,
            request.ColorPalette,
            request.ClearColorPalette,
            request.SortOrder,
            request.SortDescending,
            request.MaxGroups,
            request.ClearMaxGroups);

        await repository.UpdateAsync(entity, cancellationToken);

        var fieldNames = fields.ToDictionary(f => f.Id, f => f.Name);

        logger.CustomChartUpdated(id);
        return Result<CustomChartResponse>.Success(entity.ToResponse(
            fieldNames.GetValueOrDefault(entity.MeasureFieldId, "?"),
            entity.GroupByFieldId.HasValue ? fieldNames.GetValueOrDefault(entity.GroupByFieldId.Value, "?") : null));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.CustomChartNotFound(id);
            return Result.Failure($"Custom chart with ID '{id}' was not found.");
        }

        var action = await actionRepository.GetByIdAsync(entity.TrackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
            return Result.Failure($"Tracked action with ID '{entity.TrackedActionId}' was not found.");

        await repository.DeleteAsync(id, cancellationToken);

        logger.CustomChartDeleted(id);
        return Result.Success();
    }

    private static string? ValidateFilterTree(FilterGroupDto? group, HashSet<Guid> validFieldIds)
    {
        if (group is null) return null;

        if (group.Conditions is not null)
        {
            foreach (var condition in group.Conditions)
            {
                if (!validFieldIds.Contains(condition.FieldId))
                    return $"Filter references unknown field '{condition.FieldId}'.";

                switch (condition.Operator)
                {
                    case FilterOperator.Between when string.IsNullOrWhiteSpace(condition.Value) || string.IsNullOrWhiteSpace(condition.ValueTo):
                        return "Between operator requires both Value and ValueTo.";
                    case FilterOperator.IsEmpty or FilterOperator.IsNotEmpty:
                        break; // no value needed
                    case FilterOperator.In or FilterOperator.NotIn when string.IsNullOrWhiteSpace(condition.Value):
                        return "In/NotIn operator requires a value.";
                    default:
                        if (condition.Operator is not (FilterOperator.IsEmpty or FilterOperator.IsNotEmpty)
                            && string.IsNullOrWhiteSpace(condition.Value))
                            return $"Operator '{condition.Operator}' requires a value.";
                        break;
                }
            }
        }

        if (group.Groups is not null)
        {
            foreach (var subGroup in group.Groups)
            {
                var error = ValidateFilterTree(subGroup, validFieldIds);
                if (error is not null) return error;
            }
        }

        return null;
    }
}
