/**
 * @file ResultRepository.cs
 *
 * @brief Repository for handling ResultEntity data operations.
 *
 * This file contains the implementation of the ResultRepository class, which is responsible for handling ResultEntity data operations. It provides methods for counting domains, positive results, and results filtered by blacklist within specified time ranges asynchronously.
 *
 * The main functionalities of this file include:
 * - Counting the number of domains detected within a specified time range asynchronously.
 * - Counting the number of positive results detected within a specified time range asynchronously.
 * - Counting the number of results filtered by blacklist asynchronously.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using DAL.Entities;
using DAL.Repositories.Interfaces;
using MongoDB.Driver;

namespace DAL.Repositories;

/// <summary>
///     Repository for handling ResultEntity data operations.
/// </summary>
internal class ResultRepository(ApiDbContext dbContext) : RepositoryBase<ResultEntity>(dbContext), IResultRepository
{
    /// <summary>
    ///     Counts the number of domains detected within a specified time range asynchronously.
    /// </summary>
    /// <param name="start">Start datetime for filtering.</param>
    /// <param name="end">End datetime for filtering.</param>
    /// <returns>Number of domains detected within the specified time range.</returns>
    public async Task<long> CountDomainsFromStartToEndDateTimeAsync(DateTime start, DateTime end)
    {
        FilterDefinition<ResultEntity>? filter = Builders<ResultEntity>.Filter.And(
            Builders<ResultEntity>.Filter.Gte("Detected", start),
            Builders<ResultEntity>.Filter.Lt("Detected", end)
        );
        return await Collection.CountDocumentsAsync(filter);
    }

    /// <summary>
    ///     Counts the number of positive results detected within a specified time range asynchronously.
    /// </summary>
    /// <param name="start">Start datetime for filtering.</param>
    /// <param name="end">End datetime for filtering.</param>
    /// <returns>Number of positive results detected within the specified time range.</returns>
    public async Task<long> CountPositiveResultsFromStartToEndDateTimeAsync(DateTime start, DateTime end)
    {
        FilterDefinition<ResultEntity>? filter = Builders<ResultEntity>.Filter.And(
            Builders<ResultEntity>.Filter.Gte("Detected", start),
            Builders<ResultEntity>.Filter.Lt("Detected", end),
            Builders<ResultEntity>.Filter.Eq("DangerousBoolValue", true)
        );
        return await Collection.CountDocumentsAsync(filter);
    }

    /// <summary>
    ///     Counts the number of results filtered by blacklist asynchronously.
    /// </summary>
    /// <returns>Number of results filtered by blacklist.</returns>
    public async Task<long> CountFilteredByBlacklistAsync()
    {
        FilterDefinition<ResultEntity>? filter = Builders<ResultEntity>.Filter.Eq("DidBlacklistHit", true);
        return await Collection.CountDocumentsAsync(filter);
    }
}
