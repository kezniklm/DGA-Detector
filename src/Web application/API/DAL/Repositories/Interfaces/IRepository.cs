/**
 * @file IRepository.cs
 *
 * @brief Defines a generic repository interface for data access operations.
 *
 * This file contains the definition of the IRepository<TEntity> interface, which represents a generic repository for performing CRUD operations on entities of type TEntity. The interface defines methods for common data access operations such as fetching all entities, fetching an entity by its ID, inserting, updating, and deleting entities, as well as methods for counting entities and checking for existence.
 *
 * The main functionalities of this interface include:
 * - Retrieving all entities asynchronously.
 * - Retrieving an entity by its ID asynchronously.
 * - Counting all entities asynchronously.
 * - Inserting an entity asynchronously.
 * - Updating an entity asynchronously.
 * - Removing an entity asynchronously.
 * - Checking for the existence of an entity asynchronously.
 * - Retrieving a limited number of entities with optional search query support.
 *
 * @tparam TEntity The type of entity managed by the repository, which must implement the IEntity interface.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using DAL.Entities.Interfaces;

namespace DAL.Repositories.Interfaces;

/// <summary>
///     Generic repository interface for CRUD operations on entities.
/// </summary>
/// <typeparam name="TEntity">The type of entity.</typeparam>
public interface IRepository<TEntity> where TEntity : IEntity
{
    /// <summary>
    ///     Asynchronously retrieves all entities.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, returning a list of entities.</returns>
    Task<IList<TEntity>> GetAllAsync();

    /// <summary>
    ///     Asynchronously retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>A task that represents the asynchronous operation, returning the entity or null if not found.</returns>
    Task<TEntity?> GetByIdAsync(string id);

    /// <summary>
    ///     Asynchronously counts all entities.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, returning the count of entities.</returns>
    Task<long> CountAllAsync();

    /// <summary>
    ///     Asynchronously inserts a new entity.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>A task that represents the asynchronous operation, returning the identifier of the inserted entity.</returns>
    Task<string> InsertAsync(TEntity entity);


    /// <summary>
    ///     Asynchronously updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation, returning the identifier of the updated entity or null if
    ///     not found.
    /// </returns>
    Task<string?> UpdateAsync(TEntity entity);

    /// <summary>
    ///     Asynchronously removes an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(string id);

    /// <summary>
    ///     Asynchronously checks if an entity with the specified identifier exists.
    /// </summary>
    /// <param name="id">The identifier to check.</param>
    /// <returns>A task that represents the asynchronous operation, returning true if the entity exists, otherwise false.</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    ///     Asynchronously retrieves a limited number of entities, optionally filtered by a search query.
    /// </summary>
    /// <param name="skip">The number of entities to skip.</param>
    /// <param name="limit">The maximum number of entities to retrieve.</param>
    /// <param name="searchQuery">An optional search query to filter entities.</param>
    /// <returns>A task that represents the asynchronous operation, returning the list of entities.</returns>
    Task<IEnumerable<TEntity>> GetLimitOrGetAllAsync(int skip, int limit, string? searchQuery = null);
}
