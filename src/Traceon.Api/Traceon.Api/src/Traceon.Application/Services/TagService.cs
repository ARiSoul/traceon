using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Contracts.Tags;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class TagService(
    ITagRepository repository,
    ICurrentUserService currentUser,
    ILogger<TagService> logger) : ITagService
{
    public async Task<Result<TagResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.TagNotFound(id);
            return Result<TagResponse>.Failure($"Tag with ID '{id}' was not found.");
        }

        return Result<TagResponse>.Success(entity.ToResponse());
    }

    public async Task<Result<IReadOnlyList<TagResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await repository.GetAllByUserIdAsync(currentUser.UserId, cancellationToken);
        return Result<IReadOnlyList<TagResponse>>.Success(entities.ToResponseList());
    }

    public async Task<Result<TagResponse>> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsByNameAsync(currentUser.UserId, request.Name.Trim(), cancellationToken))
            return Result<TagResponse>.Failure($"A tag with name '{request.Name.Trim()}' already exists.");

        var entity = Tag.Create(currentUser.UserId, request.Name, request.Description, request.Color);
        await repository.AddAsync(entity, cancellationToken);

        logger.TagCreated(entity.Name, entity.Id);
        return Result<TagResponse>.Success(entity.ToResponse());
    }

    public async Task<Result<TagResponse>> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.TagNotFound(id);
            return Result<TagResponse>.Failure($"Tag with ID '{id}' was not found.");
        }

        entity.Update(request.Name, request.Description, request.Color);
        await repository.UpdateAsync(entity, cancellationToken);

        logger.TagUpdated(id);
        return Result<TagResponse>.Success(entity.ToResponse());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.TagNotFound(id);
            return Result.Failure($"Tag with ID '{id}' was not found.");
        }

        await repository.DeleteAsync(id, cancellationToken);

        logger.TagDeleted(id);
        return Result.Success();
    }
}
