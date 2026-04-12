using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Contracts.ActionEntries;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class ActionEntryService(
    IActionEntryRepository entryRepository,
    ITrackedActionRepository actionRepository,
    IActionFieldRepository fieldRepository,
    ICurrentUserService currentUser,
    ILogger<ActionEntryService> logger) : IActionEntryService
{
    public async Task<Result<IQueryable<ActionEntryResponse>>> QueryByTrackedActionIdAsync(
        Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<IQueryable<ActionEntryResponse>>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var query = from e in entryRepository.Query()
                    where e.TrackedActionId == trackedActionId
                    let actionName = action.Name
                    select new ActionEntryResponse
                    {
                        Id = e.Id,
                        TrackedActionId = trackedActionId,
                        ActionName = actionName,
                        OccurredAtUtc = e.OccurredAtUtc,
                        Notes = e.Notes,
                        CreatedAtUtc = e.CreatedAtUtc,
                        FieldValues = (from f in e.Fields
                                       join af in fieldRepository.Query()
                                           on f.ActionFieldId equals af.Id
                                       select new ActionEntryFieldResponse
                                       {
                                           Id = f.Id,
                                           ActionFieldId = f.ActionFieldId,
                                           ActionFieldName = af.Name,
                                           Value = f.Value
                                       }).ToList(),
                        UpdatedAtUtc = e.UpdatedAtUtc
                    };

        return Result<IQueryable<ActionEntryResponse>>.Success(query);
    }

    public async Task<Result<ActionEntryResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await entryRepository.GetByIdWithFieldsAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ActionEntryNotFound(id);
            return Result<ActionEntryResponse>.Failure($"Action entry with ID '{id}' was not found.");
        }

        var fieldNames = await GetFieldNameMapAsync(entity.TrackedActionId, cancellationToken);
        var actionName = await GetActionNameAsync(entity.TrackedActionId, cancellationToken);
        return Result<ActionEntryResponse>.Success(entity.ToResponse(fieldNames, actionName));
    }

    public async Task<Result<IReadOnlyList<ActionEntryResponse>>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<IReadOnlyList<ActionEntryResponse>>.Failure($"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var entries = await entryRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        var fieldNames = await GetFieldNameMapAsync(trackedActionId, cancellationToken);

        IReadOnlyList<ActionEntryResponse> responses = entries
            .Select(e => e.ToResponse(fieldNames, action.Name))
            .ToList();

        return Result<IReadOnlyList<ActionEntryResponse>>.Success(responses);
    }

    public async Task<Result<ActionEntryResponse>> CreateAsync(Guid trackedActionId, CreateActionEntryRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<ActionEntryResponse>.Failure($"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var entity = ActionEntry.Create(trackedActionId, request.OccurredAtUtc, request.Notes);

        if (request.FieldValues is not null)
        {
            foreach (var fv in request.FieldValues)
                entity.SetFieldValue(fv.ActionFieldId, fv.Value);
        }

        await entryRepository.AddAsync(entity, cancellationToken);

        var fieldNames = await GetFieldNameMapAsync(trackedActionId, cancellationToken);
        logger.ActionEntryCreated(entity.Id, trackedActionId);
        return Result<ActionEntryResponse>.Success(entity.ToResponse(fieldNames, action.Name));
    }

    public async Task<Result<ActionEntryResponse>> UpdateAsync(Guid id, UpdateActionEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await entryRepository.GetByIdWithFieldsAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ActionEntryNotFound(id);
            return Result<ActionEntryResponse>.Failure($"Action entry with ID '{id}' was not found.");
        }

        entity.Update(request.OccurredAtUtc, request.Notes);

        var fieldValues = request.FieldValues?
            .Select(fv => (fv.ActionFieldId, fv.Value))
            .ToList();

        await entryRepository.UpdateAsync(entity, fieldValues, cancellationToken);

        var fieldNames = await GetFieldNameMapAsync(entity.TrackedActionId, cancellationToken);
        var actionName = await GetActionNameAsync(entity.TrackedActionId, cancellationToken);
        logger.ActionEntryUpdated(id);
        return Result<ActionEntryResponse>.Success(entity.ToResponse(fieldNames, actionName));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await entryRepository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ActionEntryNotFound(id);
            return Result.Failure($"Action entry with ID '{id}' was not found.");
        }

        await entryRepository.DeleteAsync(id, cancellationToken);

        logger.ActionEntryDeleted(id);
        return Result.Success();
    }

    public async Task<Result<BulkOperationResponse>> BulkDeleteAsync(
        Guid trackedActionId, BulkDeleteEntriesRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<BulkOperationResponse>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        // Verify all entries belong to this tracked action
        var entries = await entryRepository.GetByIdsWithFieldsAsync(request.EntryIds, cancellationToken);
        var validIds = entries
            .Where(e => e.TrackedActionId == trackedActionId)
            .Select(e => e.Id)
            .ToList();

        if (validIds.Count == 0)
            return Result<BulkOperationResponse>.Failure(
                "None of the specified entries belong to this tracked action.", ResultErrorType.Validation);

        var affected = await entryRepository.DeleteManyAsync(validIds, cancellationToken);

        logger.ActionEntriesBulkDeleted(affected, trackedActionId);
        return Result<BulkOperationResponse>.Success(new BulkOperationResponse { AffectedCount = affected });
    }

    public async Task<Result<BulkOperationResponse>> BulkUpdateFieldsAsync(
        Guid trackedActionId, BulkUpdateEntryFieldsRequest request, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<BulkOperationResponse>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var entries = await entryRepository.GetByIdsWithFieldsAsync(request.EntryIds, cancellationToken);
        var validEntries = entries
            .Where(e => e.TrackedActionId == trackedActionId)
            .ToList();

        if (validEntries.Count == 0)
            return Result<BulkOperationResponse>.Failure(
                "None of the specified entries belong to this tracked action.", ResultErrorType.Validation);

        var incomingByFieldId = request.FieldValues.Count > 0
            ? request.FieldValues.ToDictionary(fv => fv.ActionFieldId, fv => fv.Value)
            : new Dictionary<Guid, string?>();

        foreach (var entry in validEntries)
        {
            // Update occurred date if provided
            if (request.OccurredAtUtc.HasValue)
                entry.Update(request.OccurredAtUtc.Value, request.UpdateNotes ? request.Notes : entry.Notes);
            else if (request.UpdateNotes)
                entry.Update(entry.OccurredAtUtc, request.Notes);
            else
                entry.MarkUpdated();

            // Merge field values: keep existing, override only the ones in the request
            IReadOnlyList<(Guid ActionFieldId, string? Value)>? merged = null;
            if (incomingByFieldId.Count > 0)
            {
                var mergedList = entry.Fields
                    .Select(f => (f.ActionFieldId, incomingByFieldId.TryGetValue(f.ActionFieldId, out var v) ? v : f.Value))
                    .ToList();

                // Add any incoming fields that didn't exist on this entry yet
                foreach (var fv in request.FieldValues)
                {
                    if (!mergedList.Any(m => m.ActionFieldId == fv.ActionFieldId))
                        mergedList.Add((fv.ActionFieldId, fv.Value));
                }

                merged = mergedList;
            }

            await entryRepository.UpdateAsync(entry, merged, cancellationToken);
        }

        logger.ActionEntriesBulkUpdated(validEntries.Count, trackedActionId);
        return Result<BulkOperationResponse>.Success(new BulkOperationResponse { AffectedCount = validEntries.Count });
    }

    public async Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await entryRepository.GetDeletedByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            logger.ActionEntryNotFound(id);
            return Result.Failure($"Deleted action entry with ID '{id}' was not found.");
        }

        var action = await actionRepository.GetByIdAsync(entity.TrackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.ActionEntryNotFound(id);
            return Result.Failure($"Deleted action entry with ID '{id}' was not found.");
        }

        await entryRepository.RestoreAsync(id, cancellationToken);

        logger.ActionEntryRestored(id);
        return Result.Success();
    }

    private async Task<IReadOnlyDictionary<Guid, string>> GetFieldNameMapAsync(Guid trackedActionId, CancellationToken cancellationToken)
    {
        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        return fields.ToDictionary(f => f.Id, f => f.Name);
    }

    private async Task<string> GetActionNameAsync(Guid trackedActionId, CancellationToken cancellationToken)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        return action?.Name ?? "";
    }

    public IQueryable<ActionEntryResponse> QueryAll()
    {
        var userActions = actionRepository.Query()
            .Where(a => a.UserId == currentUser.UserId);

        return from e in entryRepository.Query()
               join a in userActions on e.TrackedActionId equals a.Id
               select new ActionEntryResponse
               {
                   Id = e.Id,
                   TrackedActionId = e.TrackedActionId,
                   ActionName = a.Name,
                   OccurredAtUtc = e.OccurredAtUtc,
                   Notes = e.Notes,
                   FieldValues = (from f in e.Fields
                                  join af in fieldRepository.Query()
                                      on f.ActionFieldId equals af.Id
                                  select new ActionEntryFieldResponse
                                  {
                                      Id = f.Id,
                                      ActionFieldId = f.ActionFieldId,
                                      ActionFieldName = af.Name,
                                      Value = f.Value
                                  }).ToList(),
                   CreatedAtUtc = e.CreatedAtUtc,
                   UpdatedAtUtc = e.UpdatedAtUtc
               };
    }
}
