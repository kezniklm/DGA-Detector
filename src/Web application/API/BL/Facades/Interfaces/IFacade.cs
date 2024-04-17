/**
 * @file IFacade.cs
 *
 * @brief Defines the interface for a facade used in the business logic layer.
 *
 * This file contains the definition of the IFacade interface, which serves as a contract for facades used in the business logic layer of the application.
 *
 * The main functionalities of this interface include:
 * - Retrieving all entities asynchronously.
 * - Retrieving a paginated list of entities asynchronously based on search criteria and time range.
 * - Retrieving an entity by its ID asynchronously.
 * - Getting the total number of entities asynchronously.
 * - Creating or updating an entity asynchronously.
 * - Creating an entity asynchronously.
 * - Updating an entity asynchronously.
 * - Deleting an entity asynchronously.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 */

using BL.Models.Interfaces;
using DAL.Entities.Interfaces;

namespace BL.Facades.Interfaces;

/// <summary>
///     Represents a facade interface for handling operations between business logic and data access layers.
/// </summary>
/// <typeparam name="TEntity">The type of entity handled by the facade.</typeparam>
/// <typeparam name="TModel">The type of model handled by the facade.</typeparam>
public interface IFacade<TEntity, TModel>
    where TEntity : class, IEntity
    where TModel : class, IModel
{
    /// <summary>
    ///     Retrieves all entities asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a list of models.</returns>
    Task<List<TModel>> GetAllAsync();

    /// <summary>
    ///     Retrieves a paginated list of entities asynchronously based on the provided parameters.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="searchQuery">The search query (optional).</param>
    /// <param name="startTime">The start time filter (optional).</param>
    /// <param name="endTime">The end time filter (optional).</param>
    /// <returns>A task representing the asynchronous operation, containing a list of models.</returns>
    Task<List<TModel>> GetEntriesPerPageAsync(int pageNumber, int pageSize,
        string? searchQuery = null, DateTime? startTime = null, DateTime? endTime = null);

    /// <summary>
    ///     Retrieves an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>A task representing the asynchronous operation, containing the model if found, otherwise null.</returns>
    Task<TModel?> GetByIdAsync(string id);

    /// <summary>
    ///     Retrieves the number of all entities asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the count of entities.</returns>
    Task<long> GetNumberOfAllAsync();

    /// <summary>
    ///     Creates or updates an entity asynchronously.
    /// </summary>
    /// <param name="model">The model representing the entity to be created or updated.</param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the unique identifier of the created or updated
    ///     entity.
    /// </returns>
    Task<string?> CreateOrUpdateAsync(TModel model);

    /// <summary>
    ///     Creates a new entity asynchronously.
    /// </summary>
    /// <param name="model">The model representing the entity to be created.</param>
    /// <returns>A task representing the asynchronous operation, containing the unique identifier of the created entity.</returns>
    Task<string> CreateAsync(TModel model);

    /// <summary>
    ///     Updates an existing entity asynchronously.
    /// </summary>
    /// <param name="model">The model representing the entity to be updated.</param>
    /// <returns>A task representing the asynchronous operation, containing the unique identifier of the updated entity.</returns>
    Task<string?> UpdateAsync(TModel model);

    /// <summary>
    ///     Deletes an entity asynchronously by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(string id);
}
