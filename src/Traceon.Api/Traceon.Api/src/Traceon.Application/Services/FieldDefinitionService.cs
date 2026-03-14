using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Contracts.FieldDefinitions;
using Traceon.Application.Interfaces;
using Traceon.Application.Logging;
using Traceon.Application.Mapping;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class FieldDefinitionService(
    IFieldDefinitionRepository repository,
    ICurrentUserService currentUser,
    ILogger<FieldDefinitionService> logger) : IFieldDefinitionService
{
    public IQueryable<FieldDefinitionResponse> QueryAll()
    {
        return repository.Query()
            .Where(fd => fd.UserId == currentUser.UserId)
            .Select(fd => new FieldDefinitionResponse(
                fd.Id, fd.DefaultName, fd.DefaultDescription, fd.Type,
                fd.DropdownValues, fd.DefaultMaxValue, fd.DefaultMinValue,
                fd.DefaultIsRequired, fd.DefaultValue, fd.CreatedAtUtc, fd.UpdatedAtUtc));
    }

    public async Task<Result<FieldDefinitionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.FieldDefinitionNotFound(id);
            return Result<FieldDefinitionResponse>.Failure($"Field definition with ID '{id}' was not found.");
        }

        return Result<FieldDefinitionResponse>.Success(entity.ToResponse());
    }

    public async Task<Result<IReadOnlyList<FieldDefinitionResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await repository.GetAllByUserIdAsync(currentUser.UserId, cancellationToken);
        return Result<IReadOnlyList<FieldDefinitionResponse>>.Success(entities.ToResponseList());
    }

    public async Task<Result<FieldDefinitionResponse>> CreateAsync(CreateFieldDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = FieldDefinition.Create(
            currentUser.UserId,
            request.DefaultName,
            request.Type,
            request.DefaultDescription,
            request.DropdownValues,
            request.DefaultMaxValue,
            request.DefaultMinValue,
            request.DefaultIsRequired,
            request.DefaultValue);

        await repository.AddAsync(entity, cancellationToken);

        logger.FieldDefinitionCreated(entity.DefaultName, entity.Id);
        return Result<FieldDefinitionResponse>.Success(entity.ToResponse());
    }

    public async Task<Result<FieldDefinitionResponse>> UpdateAsync(Guid id, UpdateFieldDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.FieldDefinitionNotFound(id);
            return Result<FieldDefinitionResponse>.Failure($"Field definition with ID '{id}' was not found.");
        }

        entity.Update(
            request.DefaultName,
            request.Type,
            request.DefaultDescription,
            request.DropdownValues,
            request.DefaultMaxValue,
            request.DefaultMinValue,
            request.DefaultIsRequired,
            request.DefaultValue);

        await repository.UpdateAsync(entity, cancellationToken);

        logger.FieldDefinitionUpdated(id);
        return Result<FieldDefinitionResponse>.Success(entity.ToResponse());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.FieldDefinitionNotFound(id);
            return Result.Failure($"Field definition with ID '{id}' was not found.");
        }

        await repository.DeleteAsync(id, cancellationToken);

        logger.FieldDefinitionDeleted(id);
        return Result.Success();
    }
}
