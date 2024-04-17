/**
 * @file IResultRepository.cs
 *
 * @brief Defines the interface for the ResultRepository in the DAL.
 *
 * This file contains the definition of the IResultRepository interface, which extends the IRepository<ResultEntity> interface. It specifies the methods for interacting with the database to retrieve result data.
 *
 * The main functionalities of this interface include:
 * - Counting the number of results filtered by blacklist asynchronously.
 * - Counting positive results within a specified time range asynchronously.
 * - Counting domains within a specified time range asynchronously.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using DAL.Entities;

namespace DAL.Repositories.Interfaces;

/// <summary>
///     Interface for interacting with ResultEntity data in the repository.
/// </summary>
public interface IResultRepository : IRepository<ResultEntity>
{
    /// <summary>
    ///     Asynchronously counts the number of results filtered by blacklist.
    /// </summary>
    /// <returns>The count of results filtered by blacklist.</returns>
    Task<long> CountFilteredByBlacklistAsync();

    /// <summary>
    ///     Asynchronously counts the number of positive results within a specified time range.
    /// </summary>
    /// <param name="start">The start date and time of the range.</param>
    /// <param name="end">The end date and time of the range.</param>
    /// <returns>The count of positive results within the specified time range.</returns>
    Task<long> CountPositiveResultsFromStartToEndDateTimeAsync(DateTime start, DateTime end);

    /// <summary>
    ///     Asynchronously counts the number of unique domains within a specified time range.
    /// </summary>
    /// <param name="start">The start date and time of the range.</param>
    /// <param name="end">The end date and time of the range.</param>
    /// <returns>The count of unique domains within the specified time range.</returns>
    Task<long> CountDomainsFromStartToEndDateTimeAsync(DateTime start, DateTime end);
}
