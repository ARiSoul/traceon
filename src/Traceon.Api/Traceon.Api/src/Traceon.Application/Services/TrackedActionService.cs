using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Contracts.TrackedActions;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Contracts.Tags;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class TrackedActionService(
    ITrackedActionRepository repository,
    ITagRepository tagRepository,
    IActionEntryRepository entryRepository,
    ICurrentUserService currentUser,
    ILogger<TrackedActionService> logger) : ITrackedActionService
{
    public IQueryable<TrackedActionResponse> QueryAll()
    {
        return repository.Query()
        .Where(a => a.UserId == currentUser.UserId)
        .Select(a => new TrackedActionResponse
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            Tags = a.Tags.Select(t => new TrackedActionTagSummary
            {
                Name = t.Tag.Name,
                Color = t.Tag.Color
            }).ToList(),
            FieldCount = a.Fields.Count,
            EntryCount = entryRepository.Query().Count(e => e.TrackedActionId == a.Id),
            CreatedAtUtc = a.CreatedAtUtc,
            UpdatedAtUtc = a.UpdatedAtUtc
        });
    }

    public async Task<Result<TrackedActionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(id);
            return Result<TrackedActionResponse>.Failure($"Tracked action with ID '{id}' was not found.");
        }

        return Result<TrackedActionResponse>.Success(entity.ToResponse());
    }

    public async Task<Result<IReadOnlyList<TrackedActionResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await repository.GetAllByUserIdAsync(currentUser.UserId, cancellationToken);
        return Result<IReadOnlyList<TrackedActionResponse>>.Success(entities.ToResponseList());
    }

    public async Task<Result<TrackedActionResponse>> CreateAsync(CreateTrackedActionRequest request, CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsByNameAsync(currentUser.UserId, request.Name.Trim(), cancellationToken))
        {
            logger.TrackedActionDuplicateName(request.Name.Trim(), currentUser.UserId);
            return Result<TrackedActionResponse>.Failure($"A tracked action with name '{request.Name.Trim()}' already exists.");
        }

        var entity = TrackedAction.Create(currentUser.UserId, request.Name, request.Description);
        await repository.AddAsync(entity, cancellationToken);

        logger.TrackedActionCreated(entity.Name, entity.Id, currentUser.UserId);
        return Result<TrackedActionResponse>.Success(entity.ToResponse());
    }

    public async Task<Result<TrackedActionResponse>> UpdateAsync(Guid id, UpdateTrackedActionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(id);
            return Result<TrackedActionResponse>.Failure($"Tracked action with ID '{id}' was not found.");
        }

        entity.Update(request.Name, request.Description);
        await repository.UpdateAsync(entity, cancellationToken);

        logger.TrackedActionUpdated(id);
        return Result<TrackedActionResponse>.Success(entity.ToResponse());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(id);
            return Result.Failure($"Tracked action with ID '{id}' was not found.");
        }

        await repository.DeleteAsync(id, cancellationToken);

        logger.TrackedActionDeleted(id);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<TagResponse>>> GetTagsAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        var action = await repository.GetByIdWithTagsAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result<IReadOnlyList<TagResponse>>.Failure($"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var tagIds = action.Tags.Select(t => t.TagId).ToList();
        var tags = await tagRepository.GetByIdsAsync(tagIds, cancellationToken);
        var response = tags.Select(t => t.ToResponse()).ToList();

        return Result<IReadOnlyList<TagResponse>>.Success(response);
    }

    public async Task<Result> AddTagAsync(Guid trackedActionId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var action = await repository.GetByIdWithTagsAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result.Failure($"Tracked action with ID '{trackedActionId}' was not found.");
        }

        var tag = await tagRepository.GetByIdAsync(tagId, cancellationToken);

        if (tag is null || tag.UserId != currentUser.UserId)
        {
            logger.TagNotFound(tagId);
            return Result.Failure($"Tag with ID '{tagId}' was not found.");
        }

        action.AddTag(tag);
        await repository.UpdateAsync(action, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveTagAsync(Guid trackedActionId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var action = await repository.GetByIdWithTagsAsync(trackedActionId, cancellationToken);

        if (action is null || action.UserId != currentUser.UserId)
        {
            logger.TrackedActionNotFound(trackedActionId);
            return Result.Failure($"Tracked action with ID '{trackedActionId}' was not found.");
        }

        action.RemoveTag(tagId);
        await repository.UpdateAsync(action, cancellationToken);

        return Result.Success();
    }
}
