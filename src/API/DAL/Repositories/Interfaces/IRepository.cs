using DAL.Entities.Interfaces;

namespace DAL.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : IEntity
{
    Task<IList<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<Guid> InsertAsync(TEntity entity);
    Task<Guid?> UpdateAsync(TEntity entity);
    Task RemoveAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IList<TEntity>> GetMaxOrGetAllAsync(int max, int page);
    Task<IList<TEntity>> SearchByNameAsync(string name);
}
