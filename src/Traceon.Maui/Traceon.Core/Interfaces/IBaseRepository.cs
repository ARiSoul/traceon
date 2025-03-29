namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface IBaseRepository<T> 
    where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
