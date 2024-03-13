using AutoMapper;
using BL.Models.Interfaces;
using DAL.Entities.Interfaces;
using DAL.Repositories.Interfaces;
using MongoDB.Bson;

namespace BL.Facades;

internal abstract class FacadeBase<TListModel, TDetailModel, TEntity>(IRepository<TEntity> repository, IMapper mapper)
    where TListModel : class, IModel
    where TDetailModel : class, IModel
    where TEntity : class, IEntity
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    private readonly IRepository<TEntity> _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));

    public virtual async Task<List<TListModel>> GetAllAsync()
    {
        IList<TEntity> entities = await _repository.GetAllAsync();
        return _mapper.Map<List<TListModel>>(entities);
    }

    public virtual async Task<List<TListModel>> GetMaxOrGetAllAsync(int max, int page)
    {
        IList<TEntity> entities = await _repository.GetMaxOrGetAllAsync(max, page);
        return _mapper.Map<List<TListModel>>(entities);
    }

    public virtual async Task<TDetailModel> GetByIdAsync(ObjectId id)
    {
        TEntity? entity = await _repository.GetByIdAsync(id);
        return _mapper.Map<TDetailModel>(entity);
    }

    public virtual async Task<ObjectId?> CreateOrUpdateAsync(TDetailModel model) =>
        await _repository.ExistsAsync(model.Id)
            ? await UpdateAsync(model)
            : await CreateAsync(model);

    public virtual async Task<ObjectId> CreateAsync(TDetailModel model)
    {
        TEntity? entity = _mapper.Map<TEntity>(model);
        return await _repository.InsertAsync(entity);
    }

    public virtual async Task<ObjectId?> UpdateAsync(TDetailModel model)
    {
        TEntity? entity = _mapper.Map<TEntity>(model);
        return await _repository.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(ObjectId id) => await _repository.RemoveAsync(id);
}
