using DAL.Entities.Interfaces;
using MongoDB.Bson;

namespace DAL.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : IEntity
{
    Task<IList<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(ObjectId id);
    Task<ObjectId> InsertAsync(TEntity entity);
    Task<ObjectId?> UpdateAsync(TEntity entity);
    Task RemoveAsync(ObjectId id);
    Task<bool> ExistsAsync(ObjectId id);
    Task<IList<TEntity>> GetMaxOrGetAllAsync(int max, int page);
    Task<IList<TEntity>> SearchByNameAsync(string name);
}
