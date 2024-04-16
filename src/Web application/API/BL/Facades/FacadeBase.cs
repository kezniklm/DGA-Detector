using System.Reflection;
using AutoMapper;
using BL.Models.Interfaces;
using DAL.Entities.Interfaces;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

internal abstract class FacadeBase<TModel, TEntity>(IRepository<TEntity> repository, IMapper mapper)
    where TModel : class, IModel
    where TEntity : class, IEntity
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    protected readonly IRepository<TEntity> Repository =
        repository ?? throw new ArgumentNullException(nameof(repository));

    public virtual async Task<List<TModel>> GetAllAsync()
    {
        IList<TEntity> entities = await Repository.GetAllAsync();
        return _mapper.Map<List<TModel>>(entities);
    }

    public virtual async Task<List<TModel>> GetEntriesPerPageAsync(int pageNumber, int pageSize,
        string? searchQuery = null, DateTime? startTime = null, DateTime? endTime = null)
    {
        int skip = (pageNumber - 1) * pageSize;

        IEnumerable<TEntity> entities = await Repository.GetLimitOrGetAllAsync(skip, pageSize, searchQuery);

        if (startTime.HasValue && endTime.HasValue)
        {
            entities = FilterEntitiesByDate(entities, startTime.Value, endTime.Value);
        }

        return _mapper.Map<List<TModel>>(entities);
    }

    private static IEnumerable<TEntity> FilterEntitiesByDate(IEnumerable<TEntity> entities, DateTime startTime,
        DateTime endTime)
    {
        List<TEntity> filteredEntities = new();

        foreach (TEntity entity in entities)
        {
            PropertyInfo? addedProp = entity.GetType().GetProperty("Added");
            PropertyInfo? detectedProp = entity.GetType().GetProperty("Detected");

            DateTime? date = null;

            if (addedProp is not null && addedProp.PropertyType == typeof(DateTime))
            {
                date = (DateTime?)addedProp.GetValue(entity);
            }
            else if (detectedProp is not null && detectedProp.PropertyType == typeof(DateTime))
            {
                date = (DateTime?)detectedProp.GetValue(entity);
            }

            if (date >= startTime && date <= endTime)
            {
                filteredEntities.Add(entity);
            }
        }

        return filteredEntities;
    }

    public virtual async Task<TModel?> GetByIdAsync(string id)
    {
        TEntity? entity = await Repository.GetByIdAsync(id);
        return _mapper.Map<TModel>(entity);
    }

    public virtual async Task<long> GetNumberOfAllAsync() => await Repository.CountAllAsync();

    public virtual async Task<string?> CreateOrUpdateAsync(TModel model) =>
        await Repository.ExistsAsync(model.Id)
            ? await UpdateAsync(model)
            : await CreateAsync(model);

    public virtual async Task<string> CreateAsync(TModel model)
    {
        TEntity? entity = _mapper.Map<TEntity>(model);
        return await Repository.InsertAsync(entity);
    }

    public virtual async Task<string?> UpdateAsync(TModel model)
    {
        TEntity? entity = _mapper.Map<TEntity>(model);
        return await Repository.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(string id) => await Repository.RemoveAsync(id);
}
