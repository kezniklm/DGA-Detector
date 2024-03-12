using BL.Models.Interfaces;
using DAL.Entities.Interfaces;
using MongoDB.Bson;

namespace BL.Facades.Interfaces;

public interface IFacade<TEntity,TListModel, TDetailModel>
    where TEntity : class, IEntity
    where TListModel : IModel
    where TDetailModel : class, IModel
{
    Task<List<TListModel>> GetAllAsync();
    Task<List<TListModel>> GetMaxOrGetAllAsync(int max, int page);
    Task<TDetailModel> GetByIdAsync(ObjectId id);
    Task<ObjectId?> CreateOrUpdateAsync(TDetailModel model);
    Task<ObjectId> CreateAsync(TDetailModel model);
    Task<ObjectId?> UpdateAsync(TDetailModel model);
    Task DeleteAsync(ObjectId id);
}
