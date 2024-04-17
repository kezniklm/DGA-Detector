/**
 * @file RepositoryBase.cs
 *
 * @brief Implements common CRUD operations for entities.
 *
 * This class provides a base implementation for repository classes handling CRUD operations for entities.
 *
 * Main Features:
 * - Implements common CRUD operations (Create, Read, Update, Delete) for entities.
 * - Supports asynchronous operations for database interaction.
 * - Provides methods for searching, counting, and pagination of entities.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Common.Exceptions;
using DAL.Entities.Interfaces;
using DAL.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Repositories;

/// <summary>
///     Base repository class implementing common CRUD operations for entities.
/// </summary>
/// <typeparam name="TEntity">Type of entity.</typeparam>
public class RepositoryBase<TEntity> : IRepository<TEntity>, IDisposable
    where TEntity : class, IEntity
{
    protected readonly IMongoCollection<TEntity> Collection;

    /// <summary>
    ///     Constructs a new instance of RepositoryBase.
    /// </summary>
    /// <param name="dbContext">Instance of the database context.</param>
    public RepositoryBase(ApiDbContext dbContext)
    {
        string collectionName = typeof(TEntity).Name.EndsWith("Entity")
            ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - "Entity".Length)
            : typeof(TEntity).Name;

        Collection = dbContext.Database.GetCollection<TEntity>(collectionName);
    }

    /// <summary>
    ///     Disposes the repository instance.
    /// </summary>
    public virtual void Dispose() => GC.SuppressFinalize(this);

    /// <summary>
    ///     Gets all entities asynchronously.
    /// </summary>
    /// <returns>A list of all entities.</returns>
    public virtual async Task<IList<TEntity>> GetAllAsync() =>
        await Collection.Find(Builders<TEntity>.Filter.Empty).ToListAsync();

    /// <summary>
    ///     Gets an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <returns>The entity with the specified ID, or null if not found.</returns>
    public virtual async Task<TEntity?> GetByIdAsync(string id) =>
        await Collection.Find(entity => entity.Id == id).SingleOrDefaultAsync();

    /// <summary>
    ///     Inserts a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>The ID of the inserted entity.</returns>
    public virtual async Task<string> InsertAsync(TEntity entity)
    {
        await Collection.InsertOneAsync(entity);
        return entity.Id;
    }

    /// <summary>
    ///     Updates an existing entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The ID of the updated entity, or null if the update failed.</returns>
    public virtual async Task<string?> UpdateAsync(TEntity entity)
    {
        ReplaceOneResult? result = await Collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            return entity.Id;
        }

        return null;
    }

    /// <summary>
    ///     Removes an entity asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to remove.</param>
    /// <exception cref="InvalidDeleteException">Thrown when the entity cannot be deleted because it does not exist.</exception>
    public virtual async Task RemoveAsync(string id)
    {
        DeleteResult? result = await Collection.DeleteOneAsync(entity => entity.Id == id);
        if (result.DeletedCount <= 0)
        {
            throw new InvalidDeleteException("Entity cannot be deleted because it does not exist");
        }
    }

    /// <summary>
    ///     Checks if an entity with the specified ID exists asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to check.</param>
    /// <returns>True if the entity exists, otherwise false.</returns>
    public virtual async Task<bool> ExistsAsync(string id) =>
        await Collection.Find(entity => entity.Id == id).AnyAsync();

    /// <summary>
    ///     Gets entities with optional search and pagination asynchronously.
    /// </summary>
    /// <param name="skip">Number of entities to skip.</param>
    /// <param name="limit">Maximum number of entities to return.</param>
    /// <param name="searchQuery">Optional search query.</param>
    /// <returns>A list of entities matching the criteria.</returns>
    public virtual async Task<IEnumerable<TEntity>> GetLimitOrGetAllAsync(int skip, int limit,
        string? searchQuery = null)
    {
        FilterDefinition<TEntity>? filter = Builders<TEntity>.Filter.Empty;

        if (!string.IsNullOrEmpty(searchQuery))
        {
            filter = Builders<TEntity>.Filter.Or(
                Builders<TEntity>.Filter.Regex(x => x.DomainName, new BsonRegularExpression(searchQuery, "i")));
        }

        return await Collection.Find(filter)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    /// <summary>
    ///     Counts all entities asynchronously.
    /// </summary>
    /// <returns>The total number of entities.</returns>
    public virtual async Task<long> CountAllAsync() => await Collection.CountDocumentsAsync(new BsonDocument());

    /// <summary>
    ///     Searches entities by name asynchronously.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>A list of entities matching the name.</returns>
    /// <exception cref="ArgumentException">Thrown when the name is null or empty.</exception>
    public virtual async Task<IList<TEntity>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        FilterDefinition<TEntity>?
            filter = Builders<TEntity>.Filter.Regex("Name", new BsonRegularExpression(name, "i"));
        return await Collection.Find(filter).ToListAsync();
    }
}
