using Common.Exceptions;
using DAL.Entities.Interfaces;
using DAL.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Repositories;

public class RepositoryBase<TEntity> : IRepository<TEntity>, IDisposable
    where TEntity : class, IEntity
{
    protected readonly IMongoCollection<TEntity> Collection;

    public RepositoryBase(ApiDbContext dbContext)
    {
        string collectionName = typeof(TEntity).Name.EndsWith("Entity")
            ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - "Entity".Length)
            : typeof(TEntity).Name;

        Collection = dbContext.Database.GetCollection<TEntity>(collectionName);
    }

    public virtual void Dispose() => GC.SuppressFinalize(this);

    public virtual async Task<IList<TEntity>> GetAllAsync() =>
        await Collection.Find(Builders<TEntity>.Filter.Empty).ToListAsync();

    public virtual async Task<TEntity?> GetByIdAsync(string id) =>
        await Collection.Find(entity => entity.Id == id).SingleOrDefaultAsync();

    public virtual async Task<string> InsertAsync(TEntity entity)
    {
        await Collection.InsertOneAsync(entity);
        return entity.Id;
    }

    public virtual async Task<string?> UpdateAsync(TEntity entity)
    {
        ReplaceOneResult? result = await Collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            return entity.Id;
        }

        return null;
    }

    public virtual async Task RemoveAsync(string id)
    {
        DeleteResult? result = await Collection.DeleteOneAsync(entity => entity.Id == id);
        if (result.DeletedCount <= 0)
        {
            throw new InvalidDeleteException("Entity cannot be deleted because it does not exist");
        }
    }

    public virtual async Task<bool> ExistsAsync(string id) =>
        await Collection.Find(entity => entity.Id == id).AnyAsync();

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

    public virtual async Task<long> CountAllAsync() => await Collection.CountDocumentsAsync(new BsonDocument());


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
