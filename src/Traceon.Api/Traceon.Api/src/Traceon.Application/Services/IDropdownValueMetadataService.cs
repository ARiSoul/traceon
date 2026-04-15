using Traceon.Application.Common;
using Traceon.Contracts.DropdownValues;

namespace Traceon.Application.Services;

public interface IDropdownValueMetadataService
{
    Task<Result<IReadOnlyList<DropdownValueMetadataFieldResponse>>> GetFieldsByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DropdownValueMetadataFieldResponse>>> GetAllFieldsAsync(CancellationToken cancellationToken = default);
    Task<Result<DropdownValueMetadataFieldResponse>> CreateFieldAsync(CreateDropdownValueMetadataFieldRequest request, CancellationToken cancellationToken = default);
    Task<Result<DropdownValueMetadataFieldResponse>> UpdateFieldAsync(Guid id, UpdateDropdownValueMetadataFieldRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteFieldAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> ReorderFieldsAsync(Guid fieldDefinitionId, List<Guid> orderedIds, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<DropdownValueMetadataValueEntry>>> GetValuesAsync(Guid dropdownValueId, CancellationToken cancellationToken = default);
    Task<Result> UpsertValuesAsync(Guid dropdownValueId, List<DropdownValueMetadataValueEntry> values, CancellationToken cancellationToken = default);
}
