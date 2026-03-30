using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IFieldDefinitionRepository
{
    IQueryable<FieldDefinition> Query();
    Task<FieldDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FieldDefinition>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(FieldDefinition fieldDefinition, CancellationToken cancellationToken = default);
    Task UpdateAsync(FieldDefinition fieldDefinition, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
