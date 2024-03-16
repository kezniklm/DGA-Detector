using DAL.Entities;
using DAL.Repositories.Interfaces;
using MongoDB.Driver;

namespace DAL.Repositories;

internal class ResultRepository(ApiDbContext dbContext) : RepositoryBase<ResultEntity>(dbContext), IResultRepository
{
    public async Task<long> CountDomainsFromStartToEndDateTimeAsync(DateTime start, DateTime end)
    {
        FilterDefinition<ResultEntity>? filter = Builders<ResultEntity>.Filter.And(
            Builders<ResultEntity>.Filter.Gte("Detected", start),
            Builders<ResultEntity>.Filter.Lt("Detected", end)
        );
        return await Collection.CountDocumentsAsync(filter);
    }

    public async Task<long> CountPositiveResultsFromStartToEndDateTimeAsync(DateTime start, DateTime end)
    {
        FilterDefinition<ResultEntity>? filter = Builders<ResultEntity>.Filter.And(
            Builders<ResultEntity>.Filter.Gte("Detected", start),
            Builders<ResultEntity>.Filter.Lt("Detected", end),
            Builders<ResultEntity>.Filter.Eq("DangerousBoolValue", true)
        );
        return await Collection.CountDocumentsAsync(filter);
    }

    public async Task<long> CountFilteredByBlacklistAsync()
    {
        FilterDefinition<ResultEntity>? filter = Builders<ResultEntity>.Filter.Eq("DidBlacklistHit", true);
        return await Collection.CountDocumentsAsync(filter);
    }
}
