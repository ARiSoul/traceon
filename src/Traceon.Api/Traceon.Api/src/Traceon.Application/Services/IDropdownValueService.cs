using Traceon.Application.Common;
using Traceon.Contracts.DropdownValues;

namespace Traceon.Application.Services;

public interface IDropdownValueService
{
    Task<Result<IReadOnlyList<DropdownValueResponse>>> GetByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);
    Task<Result<DropdownValueResponse>> RenameAsync(Guid id, string newValue, CancellationToken cancellationToken = default);
    Task<Result> ReorderAsync(Guid fieldDefinitionId, List<Guid> orderedIds, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Syncs the DropdownValues table from the FieldDefinition.DropdownValues pipe-delimited string.
    /// Called during migration or when the legacy append API is used.
    /// </summary>
    Task<Result> SyncFromFieldDefinitionAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);
}