using Arisoul.Core.Root.Models;

namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface IBaseRepository<TEntity, TModel> 
    where TEntity : class, IEntityWithId
    where TModel : class, IEntityWithId
{
    Task<Result<IEnumerable<TModel>>> GetAllAsync();
    Task<Result<TModel>> GetByIdAsync(Guid id);
    Task<Result> CreateAsync(TModel model);
    Task<Result> UpdateAsync(TModel model);
    Task<Result> DeleteAsync(Guid id);
    IEnumerable<TModel> MapEntityToModelCollection(IEnumerable<TEntity> entities);
    TModel MapEntityToModel(TEntity entity);
    TEntity MapModelToEntity(TModel model);
    void OnAfterUpdateValuesInEntity(TModel model, TEntity updatedEntity);
    void OnAfterAddEntity(TModel model, TEntity createdEntity);
}
