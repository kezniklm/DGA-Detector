using DAL.Entities.Interfaces;
using MongoDB.Bson;

namespace DAL.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : IEntity
{
    Task<IList<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(ObjectId id);
    Task<long> CountAllAsync();
    Task<ObjectId> InsertAsync(TEntity entity);
    Task<ObjectId?> UpdateAsync(TEntity entity);
    Task RemoveAsync(ObjectId id);
    Task<bool> ExistsAsync(ObjectId id);
    Task<IEnumerable<TEntity>> GetLimitOrGetAllAsync(int skip, int limit, string? searchQuery = null);
}
