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
                    select new ActionEntryResponse
                    {
                        Id = e.Id,
                        TrackedActionId = trackedActionId,
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
        return Result<ActionEntryResponse>.Success(entity.ToResponse(fieldNames));
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
            .Select(e => e.ToResponse(fieldNames))
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
        return Result<ActionEntryResponse>.Success(entity.ToResponse(fieldNames));
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

        if (request.FieldValues is not null)
        {
            entity.ClearFields();

            foreach (var fv in request.FieldValues)
                entity.SetFieldValue(fv.ActionFieldId, fv.Value);
        }

        await entryRepository.UpdateAsync(entity, cancellationToken);

        var fieldNames = await GetFieldNameMapAsync(entity.TrackedActionId, cancellationToken);
        logger.ActionEntryUpdated(id);
        return Result<ActionEntryResponse>.Success(entity.ToResponse(fieldNames));
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

    private async Task<IReadOnlyDictionary<Guid, string>> GetFieldNameMapAsync(Guid trackedActionId, CancellationToken cancellationToken)
    {
        var fields = await fieldRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        return fields.ToDictionary(f => f.Id, f => f.Name);
    }
}
