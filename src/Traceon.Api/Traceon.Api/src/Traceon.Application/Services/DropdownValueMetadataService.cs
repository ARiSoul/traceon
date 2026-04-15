using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Contracts.DropdownValues;
using Traceon.Contracts.Enums;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Application.Services;

public sealed class DropdownValueMetadataService(
    IDropdownValueMetadataFieldRepository fieldRepository,
    IDropdownValueMetadataValueRepository valueRepository,
    IDropdownValueRepository dropdownValueRepository,
    IFieldDefinitionRepository fieldDefinitionRepository,
    ICurrentUserService currentUser,
    ILogger<DropdownValueMetadataService> logger) : IDropdownValueMetadataService
{
    public async Task<Result<IReadOnlyList<DropdownValueMetadataFieldResponse>>> GetFieldsByFieldDefinitionIdAsync(
        Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result<IReadOnlyList<DropdownValueMetadataFieldResponse>>.Failure("Field definition not found.");

        var entities = await fieldRepository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var responses = entities.Select(ToResponse).ToList() as IReadOnlyList<DropdownValueMetadataFieldResponse>;
        return Result<IReadOnlyList<DropdownValueMetadataFieldResponse>>.Success(responses);
    }

    public async Task<Result<IReadOnlyList<DropdownValueMetadataFieldResponse>>> GetAllFieldsAsync(
        CancellationToken cancellationToken = default)
    {
        var fieldDefs = await fieldDefinitionRepository.GetAllByUserIdAsync(currentUser.UserId!, cancellationToken);
        var ids = fieldDefs.Select(f => f.Id).ToList();
        if (ids.Count == 0)
            return Result<IReadOnlyList<DropdownValueMetadataFieldResponse>>.Success([]);

        var entities = await fieldRepository.GetByFieldDefinitionIdsAsync(ids, cancellationToken);
        var responses = entities.Select(ToResponse).ToList() as IReadOnlyList<DropdownValueMetadataFieldResponse>;
        return Result<IReadOnlyList<DropdownValueMetadataFieldResponse>>.Success(responses);
    }

    public async Task<Result<DropdownValueMetadataFieldResponse>> CreateFieldAsync(
        CreateDropdownValueMetadataFieldRequest request, CancellationToken cancellationToken = default)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(request.FieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result<DropdownValueMetadataFieldResponse>.Failure("Field definition not found.");

        if (fieldDef.Type != FieldType.Dropdown)
            return Result<DropdownValueMetadataFieldResponse>.Failure(
                "Metadata fields can only be defined on Dropdown-type field definitions.", ResultErrorType.Validation);

        var existing = await fieldRepository.GetByFieldDefinitionIdAsync(request.FieldDefinitionId, cancellationToken);
        var trimmedName = request.Name.Trim();
        if (existing.Any(f => string.Equals(f.Name, trimmedName, StringComparison.OrdinalIgnoreCase)))
            return Result<DropdownValueMetadataFieldResponse>.Failure(
                "A metadata field with this name already exists.", ResultErrorType.Conflict);

        DropdownValueMetadataField entity;
        try
        {
            entity = DropdownValueMetadataField.Create(
                request.FieldDefinitionId,
                trimmedName,
                request.Type,
                request.Description,
                request.IsRequired,
                request.MinValue,
                request.MaxValue,
                request.DefaultValue,
                request.Unit,
                request.DropdownValues,
                request.SortOrder,
                request.DisplayStyle);
        }
        catch (ArgumentException ex)
        {
            return Result<DropdownValueMetadataFieldResponse>.Failure(ex.Message, ResultErrorType.Validation);
        }

        await fieldRepository.AddAsync(entity, cancellationToken);
        logger.LogInformation("Created dropdown value metadata field {Id} on FieldDefinition {FieldDefinitionId}",
            entity.Id, request.FieldDefinitionId);

        return Result<DropdownValueMetadataFieldResponse>.Success(ToResponse(entity));
    }

    public async Task<Result<DropdownValueMetadataFieldResponse>> UpdateFieldAsync(
        Guid id, UpdateDropdownValueMetadataFieldRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await fieldRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<DropdownValueMetadataFieldResponse>.Failure("Metadata field not found.");

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(entity.FieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result<DropdownValueMetadataFieldResponse>.Failure("Metadata field not found.");

        var trimmedName = request.Name.Trim();
        var siblings = await fieldRepository.GetByFieldDefinitionIdAsync(entity.FieldDefinitionId, cancellationToken);
        if (siblings.Any(f => f.Id != id && string.Equals(f.Name, trimmedName, StringComparison.OrdinalIgnoreCase)))
            return Result<DropdownValueMetadataFieldResponse>.Failure(
                "A metadata field with this name already exists.", ResultErrorType.Conflict);

        try
        {
            entity.Update(
                trimmedName,
                request.Type,
                request.Description,
                request.IsRequired,
                request.MinValue,
                request.MaxValue,
                request.DefaultValue,
                request.Unit,
                request.DropdownValues,
                request.DisplayStyle);
        }
        catch (ArgumentException ex)
        {
            return Result<DropdownValueMetadataFieldResponse>.Failure(ex.Message, ResultErrorType.Validation);
        }

        await fieldRepository.UpdateAsync(entity, cancellationToken);
        return Result<DropdownValueMetadataFieldResponse>.Success(ToResponse(entity));
    }

    public async Task<Result> DeleteFieldAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await fieldRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Metadata field not found.");

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(entity.FieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result.Failure("Metadata field not found.");

        await valueRepository.DeleteByMetadataFieldIdAsync(id, cancellationToken);
        await fieldRepository.DeleteAsync(id, cancellationToken);
        logger.LogInformation("Deleted dropdown value metadata field {Id}", id);
        return Result.Success();
    }

    public async Task<Result> ReorderFieldsAsync(
        Guid fieldDefinitionId, List<Guid> orderedIds, CancellationToken cancellationToken = default)
    {
        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(fieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result.Failure("Field definition not found.");

        var entities = await fieldRepository.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken);
        var entityMap = entities.ToDictionary(e => e.Id);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            if (entityMap.TryGetValue(orderedIds[i], out var entity))
            {
                entity.SetSortOrder(i);
                await fieldRepository.UpdateAsync(entity, cancellationToken);
            }
        }

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<DropdownValueMetadataValueEntry>>> GetValuesAsync(
        Guid dropdownValueId, CancellationToken cancellationToken = default)
    {
        var ownershipCheck = await EnsureDropdownValueOwnershipAsync(dropdownValueId, cancellationToken);
        if (ownershipCheck.IsFailure)
            return Result<IReadOnlyList<DropdownValueMetadataValueEntry>>.Failure(ownershipCheck.Error!, ownershipCheck.ErrorType);

        var values = await valueRepository.GetByDropdownValueIdAsync(dropdownValueId, cancellationToken);
        var entries = values
            .Select(v => new DropdownValueMetadataValueEntry(v.MetadataFieldId, v.Value))
            .ToList() as IReadOnlyList<DropdownValueMetadataValueEntry>;
        return Result<IReadOnlyList<DropdownValueMetadataValueEntry>>.Success(entries);
    }

    public async Task<Result> UpsertValuesAsync(
        Guid dropdownValueId, List<DropdownValueMetadataValueEntry> values, CancellationToken cancellationToken = default)
    {
        var ownershipCheck = await EnsureDropdownValueOwnershipAsync(dropdownValueId, cancellationToken);
        if (ownershipCheck.IsFailure)
            return ownershipCheck;

        var dropdownValue = await dropdownValueRepository.GetByIdAsync(dropdownValueId, cancellationToken);
        if (dropdownValue is null)
            return Result.Failure("Dropdown value not found.");

        var metadataFields = await fieldRepository.GetByFieldDefinitionIdAsync(dropdownValue.FieldDefinitionId, cancellationToken);
        var fieldMap = metadataFields.ToDictionary(f => f.Id);

        var valueMap = new Dictionary<Guid, string?>();
        foreach (var entry in values)
        {
            if (!fieldMap.TryGetValue(entry.MetadataFieldId, out var metaField))
                return Result.Failure(
                    $"Metadata field '{entry.MetadataFieldId}' is not defined on this dropdown's field definition.",
                    ResultErrorType.Validation);

            var validation = ValidateMetadataValue(metaField, entry.Value);
            if (validation is not null)
                return Result.Failure(validation, ResultErrorType.Validation);

            valueMap[entry.MetadataFieldId] = entry.Value;
        }

        await valueRepository.UpsertAsync(dropdownValueId, valueMap, cancellationToken);
        return Result.Success();
    }

    private async Task<Result> EnsureDropdownValueOwnershipAsync(Guid dropdownValueId, CancellationToken cancellationToken)
    {
        var dropdownValue = await dropdownValueRepository.GetByIdAsync(dropdownValueId, cancellationToken);
        if (dropdownValue is null)
            return Result.Failure("Dropdown value not found.");

        var fieldDef = await fieldDefinitionRepository.GetByIdAsync(dropdownValue.FieldDefinitionId, cancellationToken);
        if (fieldDef is null || fieldDef.UserId != currentUser.UserId)
            return Result.Failure("Dropdown value not found.");

        return Result.Success();
    }

    private static string? ValidateMetadataValue(DropdownValueMetadataField field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return field.IsRequired ? $"Metadata field '{field.Name}' is required." : null;

        var trimmed = value.Trim();

        switch (field.Type)
        {
            case FieldType.Integer:
                if (!long.TryParse(trimmed, System.Globalization.CultureInfo.InvariantCulture, out var iv))
                    return $"Metadata field '{field.Name}' must be an integer.";
                if (field.MinValue.HasValue && iv < field.MinValue.Value)
                    return $"Metadata field '{field.Name}' must be >= {field.MinValue.Value}.";
                if (field.MaxValue.HasValue && iv > field.MaxValue.Value)
                    return $"Metadata field '{field.Name}' must be <= {field.MaxValue.Value}.";
                break;

            case FieldType.Decimal:
                if (!decimal.TryParse(trimmed, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var dv))
                    return $"Metadata field '{field.Name}' must be a decimal.";
                if (field.MinValue.HasValue && dv < field.MinValue.Value)
                    return $"Metadata field '{field.Name}' must be >= {field.MinValue.Value}.";
                if (field.MaxValue.HasValue && dv > field.MaxValue.Value)
                    return $"Metadata field '{field.Name}' must be <= {field.MaxValue.Value}.";
                break;

            case FieldType.Date:
                if (!DateTime.TryParse(trimmed, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
                    return $"Metadata field '{field.Name}' must be a date.";
                break;

            case FieldType.Boolean:
                if (!bool.TryParse(trimmed, out _))
                    return $"Metadata field '{field.Name}' must be a boolean.";
                break;

            case FieldType.Dropdown:
                var allowed = (field.DropdownValues ?? string.Empty)
                    .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (allowed.Length > 0 && !allowed.Any(a => string.Equals(a, trimmed, StringComparison.OrdinalIgnoreCase)))
                    return $"Metadata field '{field.Name}' value must be one of: {string.Join(", ", allowed)}.";
                break;
        }

        return null;
    }

    internal static DropdownValueMetadataFieldResponse ToResponse(DropdownValueMetadataField entity) => new()
    {
        Id = entity.Id,
        FieldDefinitionId = entity.FieldDefinitionId,
        Name = entity.Name,
        Type = entity.Type,
        Description = entity.Description,
        IsRequired = entity.IsRequired,
        MinValue = entity.MinValue,
        MaxValue = entity.MaxValue,
        DefaultValue = entity.DefaultValue,
        Unit = entity.Unit,
        DropdownValues = entity.DropdownValues,
        SortOrder = entity.SortOrder,
        DisplayStyle = entity.DisplayStyle,
        CreatedAtUtc = entity.CreatedAtUtc,
        UpdatedAtUtc = entity.UpdatedAtUtc,
    };
}
