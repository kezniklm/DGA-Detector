using BL.Models.Interfaces;
using DAL.Entities.Interfaces;

namespace BL.Facades.Interfaces;

public interface IFacade<TEntity, TModel>
    where TEntity : class, IEntity
    where TModel : class, IModel
{
    Task<List<TModel>> GetAllAsync();
    Task<List<TModel>> GetEntriesPerPageAsync(int pageNumber, int pageSize,
        string? searchQuery = null, DateTime? startTime = null, DateTime? endTime = null);
    Task<TModel> GetByIdAsync(string id);
    Task<long> GetNumberOfAllAsync();
    Task<string?> CreateOrUpdateAsync(TModel model);
    Task<string> CreateAsync(TModel model);
    Task<string?> UpdateAsync(TModel model);
    Task DeleteAsync(string id);
}
