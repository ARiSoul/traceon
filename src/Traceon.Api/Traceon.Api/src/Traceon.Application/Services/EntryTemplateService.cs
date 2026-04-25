using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Contracts.EntryTemplates;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class EntryTemplateService(
    IEntryTemplateRepository templateRepository,
    ITrackedActionRepository actionRepository,
    ICurrentUserService currentUser,
    ILogger<EntryTemplateService> logger) : IEntryTemplateService
{
    public async Task<Result<IReadOnlyList<EntryTemplateResponse>>> GetByTrackedActionIdAsync(
        Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<IReadOnlyList<EntryTemplateResponse>>.Failure(
                $"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var templates = await templateRepository.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);
        IReadOnlyList<EntryTemplateResponse> responses = templates
            .Select(t => t.ToResponse())
            .ToList();

        return Result<IReadOnlyList<EntryTemplateResponse>>.Success(responses);
    }

    public async Task<Result<EntryTemplateResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await templateRepository.GetByIdWithFieldsAsync(id, cancellationToken);
        if (entity is null)
        {
            logger.EntryTemplateNotFound(id);
            return Result<EntryTemplateResponse>.Failure($"Entry template with ID '{id}' was not found.");
        }

        var ownership = await EnsureOwnershipAsync(entity.TrackedActionId, cancellationToken);
        if (!ownership.IsSuccess)
            return Result<EntryTemplateResponse>.Failure(ownership.Error!);

        return Result<EntryTemplateResponse>.Success(entity.ToResponse());
    }

    public async Task<Result<EntryTemplateResponse>> CreateAsync(
        Guid trackedActionId, CreateEntryTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var ownership = await EnsureOwnershipAsync(trackedActionId, cancellationToken);
        if (!ownership.IsSuccess)
            return Result<EntryTemplateResponse>.Failure(ownership.Error!);

        if (!string.IsNullOrWhiteSpace(request.Name)
            && await templateRepository.IsNameTakenAsync(trackedActionId, request.Name, excludeId: null, cancellationToken))
        {
            return Result<EntryTemplateResponse>.Failure(
                $"A template named '{request.Name.Trim()}' already exists for this action.",
                ResultErrorType.Validation);
        }

        var entity = EntryTemplate.Create(trackedActionId, request.Name, request.Notes);

        if (request.FieldValues is not null)
        {
            foreach (var fv in request.FieldValues)
                entity.SetFieldValue(fv.ActionFieldId, fv.Value);
        }

        await templateRepository.AddAsync(entity, cancellationToken);

        logger.EntryTemplateCreated(entity.Name, entity.Id, trackedActionId);
        return Result<EntryTemplateResponse>.Success(entity.ToResponse());
    }

    public async Task<Result<EntryTemplateResponse>> UpdateAsync(
        Guid id, UpdateEntryTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await templateRepository.GetByIdWithFieldsAsync(id, cancellationToken);
        if (entity is null)
        {
            logger.EntryTemplateNotFound(id);
            return Result<EntryTemplateResponse>.Failure($"Entry template with ID '{id}' was not found.");
        }

        var ownership = await EnsureOwnershipAsync(entity.TrackedActionId, cancellationToken);
        if (!ownership.IsSuccess)
            return Result<EntryTemplateResponse>.Failure(ownership.Error!);

        if (!string.IsNullOrWhiteSpace(request.Name)
            && await templateRepository.IsNameTakenAsync(entity.TrackedActionId, request.Name, excludeId: id, cancellationToken))
        {
            return Result<EntryTemplateResponse>.Failure(
                $"A template named '{request.Name.Trim()}' already exists for this action.",
                ResultErrorType.Validation);
        }

        entity.Update(request.Name, request.Notes);

        var fieldValues = request.FieldValues?
            .Select(fv => (fv.ActionFieldId, fv.Value))
            .ToList();

        await templateRepository.UpdateAsync(entity, fieldValues, cancellationToken);

        logger.EntryTemplateUpdated(id);
        return Result<EntryTemplateResponse>.Success(entity.ToResponse());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await templateRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            logger.EntryTemplateNotFound(id);
            return Result.Failure($"Entry template with ID '{id}' was not found.");
        }

        var ownership = await EnsureOwnershipAsync(entity.TrackedActionId, cancellationToken);
        if (!ownership.IsSuccess)
            return Result.Failure(ownership.Error!);

        await templateRepository.DeleteAsync(id, cancellationToken);
        logger.EntryTemplateDeleted(id);
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await templateRepository.GetDeletedByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            logger.EntryTemplateNotFound(id);
            return Result.Failure($"Deleted entry template with ID '{id}' was not found.");
        }

        var ownership = await EnsureOwnershipAsync(entity.TrackedActionId, cancellationToken);
        if (!ownership.IsSuccess)
            return Result.Failure(ownership.Error!);

        await templateRepository.RestoreAsync(id, cancellationToken);
        logger.EntryTemplateRestored(id);
        return Result.Success();
    }

    private async Task<Result> EnsureOwnershipAsync(Guid trackedActionId, CancellationToken cancellationToken)
    {
        var action = await actionRepository.GetByIdAsync(trackedActionId, cancellationToken);
        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result.Failure($"Tracked action with ID '{trackedActionId}' was not found.");
        }
        return Result.Success();
    }
}
