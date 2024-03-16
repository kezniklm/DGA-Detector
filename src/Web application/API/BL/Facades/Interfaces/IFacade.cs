using BL.Models.Interfaces;
using DAL.Entities.Interfaces;
using MongoDB.Bson;

namespace BL.Facades.Interfaces;

public interface IFacade<TEntity, TModel>
    where TEntity : class, IEntity
    where TModel : class, IModel
{
    Task<List<TModel>> GetAllAsync();
    Task<List<TModel>> GetEntriesPerPageAsync(int max, int page, string? searchQuery = null);
    Task<TModel> GetByIdAsync(ObjectId id);
    Task<long> GetNumberOfAllAsync();
    Task<ObjectId?> CreateOrUpdateAsync(TModel model);
    Task<ObjectId> CreateAsync(TModel model);
    Task<ObjectId?> UpdateAsync(TModel model);
    Task DeleteAsync(ObjectId id);
}
