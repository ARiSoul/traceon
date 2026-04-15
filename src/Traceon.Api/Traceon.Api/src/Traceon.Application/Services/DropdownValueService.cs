using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Contracts.DropdownValues;
using Traceon.Contracts.Helpers;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class DropdownValueService(
    IDropdownValueRepository repository,
    IFieldDefinitionRepository fieldDefinitionRepository,
    IDropdownValueMetadataValueRepository metadataValueRepository,
    ICurrentUserService currentUser,
    ILogger<DropdownValueService> logger) : IDropdownValueService
{
    public async Task<Result<IReadOnlyList<DropdownValueResponse>>> GetByFieldDefinitionIdAsync(
        Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result<IReadOnlyList<DropdownValueResponse>>.Failure("Field definition not found.");

        var entities = await repository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);

        var dropdownIds = entities.Select(e => e.Id).ToList();
        var metadata = dropdownIds.Count == 0
            ? []
            : await metadataValueRepository.GetByDropdownValueIdsAsync(dropdownIds, cancellationToken);
        var metadataByDropdownId = metadata
            .GroupBy(m => m.DropdownValueId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<DropdownValueMetadataValueEntry>)g
                    .Select(v => new DropdownValueMetadataValueEntry(v.MetadataFieldId, v.Value))
                    .ToList());

        var responses = entities.Select(e => ToResponse(e, metadataByDropdownId.GetValueOrDefault(e.Id, []))).ToList()
            as IReadOnlyList<DropdownValueResponse>;
        return Result<IReadOnlyList<DropdownValueResponse>>.Success(responses);
    }

    public async Task<Result<DropdownValueResponse>> RenameAsync(
        Guid id, string newValue, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<DropdownValueResponse>.Failure("Dropdown value not found.");

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(entity.FieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result<DropdownValueResponse>.Failure("Dropdown value not found.");

        var trimmed = newValue.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return Result<DropdownValueResponse>.Failure("Value cannot be empty.");

        if (trimmed == entity.Value)
            return Result<DropdownValueResponse>.Success(ToResponse(entity));

        // Check for duplicate
        var siblings = await repository.GetByFieldDefinitionIdAsync(entity.FieldDefinitionId, cancellationToken);
        if (siblings.Any(s => s.Id != id && string.Equals(s.Value, trimmed, StringComparison.OrdinalIgnoreCase)))
            return Result<DropdownValueResponse>.Failure("A dropdown value with this name already exists.");

        var oldValue = entity.Value;

        // Cascade rename across all related tables
        await repository.CascadeRenameAsync(entity.FieldDefinitionId, oldValue, trimmed, cancellationToken);

        // Rename the entity itself
        entity.Rename(trimmed);
        await repository.UpdateAsync(entity, cancellationToken);

        logger.LogInformation("Renamed dropdown value {Id} from '{OldValue}' to '{NewValue}' in FieldDefinition {FieldDefinitionId}",
            id, oldValue, trimmed, entity.FieldDefinitionId);

        return Result<DropdownValueResponse>.Success(ToResponse(entity));
    }

    public async Task<Result> ReorderAsync(
        Guid fieldDefinitionId, List<Guid> orderedIds, CancellationToken cancellationToken = default)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result.Failure("Field definition not found.");

        var entities = await repository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var entityMap = entities.ToDictionary(e => e.Id);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            if (entityMap.TryGetValue(orderedIds[i], out var entity))
            {
                entity.SetSortOrder(i);
                await repository.UpdateAsync(entity, cancellationToken);
            }
        }

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Dropdown value not found.");

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(entity.FieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result.Failure("Dropdown value not found.");

        await repository.DeleteAsync(id, cancellationToken);

        // Rebuild the pipe-delimited string
        await RebuildDropdownValuesStringAsync(entity.FieldDefinitionId, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SyncFromFieldDefinitionAsync(
        Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null)
            return Result.Failure("Field definition not found.");

        // Skip composite dropdowns (ref: prefix)
        if (fieldDef.DropdownValues?.StartsWith("ref:", StringComparison.Ordinal) == true)
            return Result.Success();

        var existingValues = await repository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var existingSet = existingValues.ToDictionary(v => v.Value, StringComparer.OrdinalIgnoreCase);

        var pipeValues = DropdownValuesHelper.Split(fieldDef.DropdownValues);
        var order = 0;

        foreach (var val in pipeValues)
        {
            if (!existingSet.ContainsKey(val))
            {
                var entity = DropdownValue.Create(fieldDefinitionId, val, order);
                await repository.AddAsync(entity, cancellationToken);
            }
            else
            {
                var existing = existingSet[val];
                if (existing.SortOrder != order)
                {
                    existing.SetSortOrder(order);
                    await repository.UpdateAsync(existing, cancellationToken);
                }
            }
            order++;
        }

        // Remove values that are no longer in the pipe string
        var pipeSet = new HashSet<string>(pipeValues, StringComparer.OrdinalIgnoreCase);
        foreach (var existing in existingValues)
        {
            if (!pipeSet.Contains(existing.Value))
            {
                await repository.DeleteAsync(existing.Id, cancellationToken);
            }
        }

        return Result.Success();
    }

    private async Task RebuildDropdownValuesStringAsync(Guid fieldDefinitionId, CancellationToken cancellationToken)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null) return;

        var values = await repository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var newPipeString = DropdownValuesHelper.Join(values.Select(v => v.Value));

        fieldDef.Update(
            fieldDef.DefaultName,
            fieldDef.Type,
            fieldDef.DefaultDescription,
            string.IsNullOrEmpty(newPipeString) ? null : newPipeString,
            fieldDef.DefaultMaxValue,
            fieldDef.DefaultMinValue,
            fieldDef.DefaultIsRequired,
            fieldDef.DefaultValue,
            fieldDef.Unit);

        await fieldDefinitionRepository.UpdateAsync(fieldDef, cancellationToken);
    }

    private static DropdownValueResponse ToResponse(DropdownValue entity) => ToResponse(entity, []);

    private static DropdownValueResponse ToResponse(DropdownValue entity, IReadOnlyList<DropdownValueMetadataValueEntry> metadata) => new()
    {
        Id = entity.Id,
        FieldDefinitionId = entity.FieldDefinitionId,
        Value = entity.Value,
        SortOrder = entity.SortOrder,
        CreatedAtUtc = entity.CreatedAtUtc,
        UpdatedAtUtc = entity.UpdatedAtUtc,
        Metadata = metadata,
    };
}