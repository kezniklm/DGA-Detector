using Common.Exceptions;
using DAL.Entities.Interfaces;
using DAL.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Repositories;

public class RepositoryBase<TEntity> : IRepository<TEntity>, IDisposable
    where TEntity : class, IEntity
{
    private readonly IMongoCollection<TEntity> _collection;

    public RepositoryBase(ApiDbContext dbContext)
    {
        string collectionName = typeof(TEntity).Name.EndsWith("Entity")
            ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - "Entity".Length)
            : typeof(TEntity).Name;

        _collection = dbContext.Database.GetCollection<TEntity>(collectionName);
    }

    public void Dispose()
    {
    }

    public virtual async Task<IList<TEntity>> GetAllAsync() =>
        await _collection.Find(Builders<TEntity>.Filter.Empty).ToListAsync();

    public virtual async Task<TEntity?> GetByIdAsync(ObjectId id) =>
        await _collection.Find(entity => entity.Id == id).SingleOrDefaultAsync();

    public virtual async Task<ObjectId> InsertAsync(TEntity entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity.Id;
    }

    public virtual async Task<ObjectId?> UpdateAsync(TEntity entity)
    {
        ReplaceOneResult? result = await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            return entity.Id;
        }

        return null;
    }

    public virtual async Task RemoveAsync(ObjectId id)
    {
        DeleteResult? result = await _collection.DeleteOneAsync(entity => entity.Id == id);
        if (result.DeletedCount <= 0)
        {
            throw new InvalidDeleteException("Entity cannot be deleted because it does not exist");
        }
    }

    public virtual async Task<bool> ExistsAsync(ObjectId id) =>
        await _collection.Find(entity => entity.Id == id).AnyAsync();

    public virtual async Task<IList<TEntity>> GetMaxOrGetAllAsync(int max, int page)
    {
        if (max <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(max), "Maximum results must be greater than 0.");
        }

        if (page < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page index must be 0 or greater.");
        }

        int skipAmount = max * page;

        return await _collection.Find(Builders<TEntity>.Filter.Empty).Skip(skipAmount).Limit(max).ToListAsync();
    }


    public async Task<IList<TEntity>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        FilterDefinition<TEntity>?
            filter = Builders<TEntity>.Filter.Regex("Name", new BsonRegularExpression(name, "i"));
        return await _collection.Find(filter).ToListAsync();
    }
}
