using Common.Exceptions;
using DAL.Entities.Interfaces;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DAL.Repositories;

public class RepositoryBase<TEntity> : IRepository<TEntity>, IDisposable
    where TEntity : class, IEntity
{
    private readonly ApiDbContext _dbContext;

    protected RepositoryBase(ApiDbContext dbContext) => _dbContext = dbContext;

    public void Dispose() => _dbContext.Dispose();

    public virtual async Task<IList<TEntity>> GetAllAsync() => await _dbContext.Set<TEntity>().ToListAsync();

    public virtual async Task<TEntity?> GetByIdAsync(Guid id) => await _dbContext.Set<TEntity>().SingleOrDefaultAsync(entity => entity.Id == id);

    public virtual async Task<Guid> InsertAsync(TEntity entity)
    {
        EntityEntry<TEntity> createdEntity = await _dbContext.Set<TEntity>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return createdEntity.Entity.Id;
    }

    public virtual async Task<Guid?> UpdateAsync(TEntity entity)
    {
        if (await ExistsAsync(entity.Id))
        {
            _dbContext.Set<TEntity>().Attach(entity);
            _dbContext.Set<TEntity>().Update(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        return null;
    }

    public virtual async Task RemoveAsync(Guid id)
    {
        TEntity? entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbContext.Set<TEntity>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new InvalidDeleteException("Entity cannot be deleted because it does not exist");
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id) => await _dbContext.Set<TEntity>().AnyAsync(entity => entity.Id == id);

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

        return await _dbContext
            .Set<TEntity>()
            .Skip(skipAmount)
            .Take(max)
            .ToListAsync();
    }

    public async Task<IList<TEntity>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .Where(entity => entity.DomainName.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();
    }
}
