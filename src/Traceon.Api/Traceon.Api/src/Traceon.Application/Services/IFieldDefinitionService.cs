using Traceon.Application.Common;
using Traceon.Contracts.FieldDefinitions;

namespace Traceon.Application.Services;

public interface IFieldDefinitionService
{
    Task<Result<FieldDefinitionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<FieldDefinitionResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<FieldDefinitionResponse>> CreateAsync(CreateFieldDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<Result<FieldDefinitionResponse>> UpdateAsync(Guid id, UpdateFieldDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
