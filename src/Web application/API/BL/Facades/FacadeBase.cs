/**
 * @file FacadeBase.cs
 *
 * @brief Base class for facade implementations in the business logic layer.
 *
 * This file contains the implementation of the FacadeBase class, which serves as a base for implementing facades in the business logic layer (BL). Facades encapsulate business logic and orchestrate interactions between the BL and data access layer (DAL).
 *
 * The main functionalities of this file include:
 * - Defining abstract methods and properties for common CRUD operations.
 * - Handling pagination and filtering of data entities.
 * - Mapping between domain models (TModel) and data entities (TEntity) using AutoMapper.
 * - Implementing generic methods for CRUD operations, such as Create, Update, Delete, GetById, GetAll, etc.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using System.Reflection;
using AutoMapper;
using BL.Models.Interfaces;
using DAL.Entities.Interfaces;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

/// <summary>
///     Base class for facades implementing common CRUD operations for a specific type of model and entity.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
internal abstract class FacadeBase<TModel, TEntity>(IRepository<TEntity> repository, IMapper mapper)
    where TModel : class, IModel
    where TEntity : class, IEntity
{
    /// <summary>
    ///     The AutoMapper instance used for mapping between models and entities.
    /// </summary>
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    /// <summary>
    ///     Gets the repository instance to interact with the data storage.
    /// </summary>
    protected readonly IRepository<TEntity> Repository =
        repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    ///     Gets all entities asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, returning a list of mapped models.</returns>
    public virtual async Task<List<TModel>> GetAllAsync()
    {
        IList<TEntity> entities = await Repository.GetAllAsync();
        return _mapper.Map<List<TModel>>(entities);
    }

    /// <summary>
    ///     Gets a page of entities asynchronously.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="searchQuery">The search query string.</param>
    /// <param name="startTime">The start time for filtering.</param>
    /// <param name="endTime">The end time for filtering.</param>
    /// <returns>A task representing the asynchronous operation, returning a list of mapped models.</returns>
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

    /// <summary>
    ///     Filters entities by date within a specified range.
    /// </summary>
    /// <param name="entities">The entities to filter.</param>
    /// <param name="startTime">The start time of the range.</param>
    /// <param name="endTime">The end time of the range.</param>
    /// <returns>The filtered entities.</returns>
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

    /// <summary>
    ///     Gets an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>A task representing the asynchronous operation, returning the mapped model.</returns>
    public virtual async Task<TModel?> GetByIdAsync(string id)
    {
        TEntity? entity = await Repository.GetByIdAsync(id);
        return _mapper.Map<TModel>(entity);
    }

    /// <summary>
    ///     Gets the number of all entities asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, returning the count of all entities.</returns>
    public virtual async Task<long> GetNumberOfAllAsync() => await Repository.CountAllAsync();

    /// <summary>
    ///     Creates or updates an entity asynchronously.
    /// </summary>
    /// <param name="model">The model to create or update.</param>
    /// <returns>A task representing the asynchronous operation, returning the ID of the created or updated entity.</returns>
    public virtual async Task<string?> CreateOrUpdateAsync(TModel model) =>
        await Repository.ExistsAsync(model.Id)
            ? await UpdateAsync(model)
            : await CreateAsync(model);

    /// <summary>
    ///     Creates an entity asynchronously.
    /// </summary>
    /// <param name="model">The model to create.</param>
    /// <returns>A task representing the asynchronous operation, returning the ID of the created entity.</returns>
    public virtual async Task<string> CreateAsync(TModel model)
    {
        TEntity? entity = _mapper.Map<TEntity>(model);
        return await Repository.InsertAsync(entity);
    }

    /// <summary>
    ///     Updates an entity asynchronously.
    /// </summary>
    /// <param name="model">The model to update.</param>
    /// <returns>A task representing the asynchronous operation, returning the ID of the updated entity.</returns>
    public virtual async Task<string?> UpdateAsync(TModel model)
    {
        TEntity? entity = _mapper.Map<TEntity>(model);
        return await Repository.UpdateAsync(entity);
    }

    /// <summary>
    ///     Deletes an entity asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task DeleteAsync(string id) => await Repository.RemoveAsync(id);
}
