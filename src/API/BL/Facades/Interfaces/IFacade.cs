using BL.Models.Interfaces;
using DAL.Entities.Interfaces;

namespace BL.Facades.Interfaces;

public interface IFacade<TEntity,TListModel, TDetailModel>
    where TEntity : class, IEntity
    where TListModel : IModel
    where TDetailModel : class, IModel
{
    Task<List<TListModel>> GetAllAsync();
    Task<List<TListModel>> GetMaxOrGetAllAsync(int max, int page);
    Task<TDetailModel> GetByIdAsync(Guid id);
    Task<Guid?> CreateOrUpdateAsync(TDetailModel model);
    Task<Guid> CreateAsync(TDetailModel model);
    Task<Guid?> UpdateAsync(TDetailModel model);
    Task DeleteAsync(Guid id);
}
