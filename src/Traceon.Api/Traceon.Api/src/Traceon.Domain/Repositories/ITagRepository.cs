using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface ITagRepository
{
    IQueryable<Tag> Query();
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string userId, string name, CancellationToken cancellationToken = default);
    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
