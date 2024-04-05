using DAL.Entities.Interfaces;

namespace DAL.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : IEntity
{
    Task<IList<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(string id);
    Task<long> CountAllAsync();
    Task<string> InsertAsync(TEntity entity);
    Task<string?> UpdateAsync(TEntity entity);
    Task RemoveAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<IEnumerable<TEntity>> GetLimitOrGetAllAsync(int skip, int limit, string? searchQuery = null);
}
