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
    IDropdownValueRepository dropdownValueRepository,
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

        // Seed DropdownValue rows from the pipe-delimited string
        await SyncDropdownValuesAsync(entity.Id, entity.DropdownValues, cancellationToken);

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

        // Keep DropdownValue rows in sync
        await SyncDropdownValuesAsync(entity.Id, entity.DropdownValues, cancellationToken);

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
            : Traceon.Contracts.Helpers.DropdownValuesHelper.Split(entity.DropdownValues);

        if (existing.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            return Result<string>.Success(entity.DropdownValues!);

        var updated = Traceon.Contracts.Helpers.DropdownValuesHelper.Append(entity.DropdownValues, trimmed);

        entity.Update(
            entity.DefaultName, entity.Type, entity.DefaultDescription,
            updated, entity.DefaultMaxValue, entity.DefaultMinValue,
            entity.DefaultIsRequired, entity.DefaultValue, entity.Unit);

        await repository.UpdateAsync(entity, cancellationToken);

        // Keep DropdownValues table in sync
        var existingRows = await dropdownValueRepository.GetByFieldDefinitionIdAsync(id, cancellationToken);
        if (!existingRows.Any(r => string.Equals(r.Value, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            var sortOrder = existingRows.Count > 0 ? existingRows.Max(r => r.SortOrder) + 1 : 0;
            var ddv = DropdownValue.Create(id, trimmed, sortOrder);
            await dropdownValueRepository.AddAsync(ddv, cancellationToken);
        }

        return Result<string>.Success(updated);
    }

    private async Task SyncDropdownValuesAsync(Guid fieldDefinitionId, string? dropdownValues, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dropdownValues) || dropdownValues.StartsWith("ref:", StringComparison.Ordinal))
            return;

        var pipeValues = Traceon.Contracts.Helpers.DropdownValuesHelper.Split(dropdownValues);
        var existingRows = await dropdownValueRepository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var existingSet = existingRows.ToDictionary(r => r.Value, StringComparer.OrdinalIgnoreCase);

        var order = 0;
        foreach (var val in pipeValues)
        {
            if (!existingSet.TryGetValue(val, out var row))
            {
                var entity = DropdownValue.Create(fieldDefinitionId, val, order);
                await dropdownValueRepository.AddAsync(entity, cancellationToken);
            }
            else if (row.SortOrder != order)
            {
                row.SetSortOrder(order);
                await dropdownValueRepository.UpdateAsync(row, cancellationToken);
            }
            order++;
        }

        // Remove rows no longer in the pipe string
        var pipeSet = new HashSet<string>(pipeValues, StringComparer.OrdinalIgnoreCase);
        foreach (var row in existingRows)
        {
            if (!pipeSet.Contains(row.Value))
                await dropdownValueRepository.DeleteAsync(row.Id, cancellationToken);
        }
    }
}
