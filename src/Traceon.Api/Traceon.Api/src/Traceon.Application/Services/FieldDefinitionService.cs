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
    IActionFieldRepository actionFieldRepository,
    ICurrentUserService currentUser,
    ILogger<FieldDefinitionService> logger) : IFieldDefinitionService
{
    public IQueryable<FieldDefinitionResponse> QueryAll()
    {
        return repository.Query()
            .Where(fd => fd.UserId == currentUser.UserId)
            .Select(fd => new FieldDefinitionResponse
            {
                CreatedAtUtc = fd.CreatedAtUtc,
                DefaultDescription = fd.DefaultDescription,
                DefaultIsRequired = fd.DefaultIsRequired,
                DefaultMaxValue = fd.DefaultMaxValue,
                DefaultMinValue = fd.DefaultMinValue,
                DefaultName = fd.DefaultName,
                DefaultValue = fd.DefaultValue,
                DropdownValues = fd.DropdownValues,
                Id = fd.Id,
                Type = fd.Type,
                Unit = fd.Unit,
                UpdatedAtUtc = fd.UpdatedAtUtc
            });
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
            request.DefaultValue,
            request.Unit);

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
            request.DefaultValue,
            request.Unit);

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

        if (await actionFieldRepository.ExistsByFieldDefinitionIdAsync(id, cancellationToken))
        {
            logger.FieldDefinitionInUse(id);
            return Result.Failure(
                $"Field definition '{entity.DefaultName}' cannot be deleted because it is used by one or more action fields.",
                ResultErrorType.Conflict);
        }

        await repository.DeleteAsync(id, cancellationToken);

        logger.FieldDefinitionDeleted(id);
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetDeletedByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.FieldDefinitionNotFound(id);
            return Result.Failure($"Deleted field definition with ID '{id}' was not found.");
        }

        await repository.RestoreAsync(id, cancellationToken);

        logger.FieldDefinitionRestored(id);
        return Result.Success();
    }

    public async Task<Result<string>> AppendDropdownValueAsync(Guid id, string value, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
        {
            logger.FieldDefinitionNotFound(id);
            return Result<string>.Failure($"Field definition with ID '{id}' was not found.");
        }

        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return Result<string>.Failure("Value cannot be empty.");

        var existing = string.IsNullOrWhiteSpace(entity.DropdownValues)
            ? []
            : entity.DropdownValues.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (existing.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            return Result<string>.Success(entity.DropdownValues!);

        var updated = existing.Length > 0
            ? $"{entity.DropdownValues},{trimmed}"
            : trimmed;

        entity.Update(
            entity.DefaultName, entity.Type, entity.DefaultDescription,
            updated, entity.DefaultMaxValue, entity.DefaultMinValue,
            entity.DefaultIsRequired, entity.DefaultValue, entity.Unit);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result<string>.Success(updated);
    }
}
