using Arisoul.Core.Root.Models;

namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface IBaseRepository<TEntity, TModel> 
    where TEntity : class, IEntityWithId
    where TModel : class, IEntityWithId
{
    Task<Result<IEnumerable<TModel>>> GetAllAsync(bool asNoTracking);
    Task<Result<TModel>> GetByIdAsync(Guid id, bool asNoTracking);
    Task<Result> CreateAsync(TModel model);
    Task<Result> UpdateAsync(TModel model);
    Task<Result> DeleteAsync(Guid id);
}
