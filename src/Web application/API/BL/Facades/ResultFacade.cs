/**
 * @file ResultFacade.cs
 *
 * @brief Implements facade for managing results.
 *
 * This file contains the implementation of the ResultFacade class, which serves as a facade for managing results. It provides methods for retrieving counts of domains, positive detection results, and filtered results by blacklist.
 *
 * The main functionalities of this class include:
 * - Retrieving the number of domains detected today.
 * - Retrieving the number of positive detection results today.
 * - Retrieving the count of results filtered by blacklist.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using AutoMapper;
using BL.Facades.Interfaces;
using BL.Models.Result;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

/// <summary>
///     Facade class for managing results.
/// </summary>
internal class ResultFacade(IResultRepository repository, IMapper mapper)
    : FacadeBase<ResultModel, ResultEntity>(repository, mapper), IResultFacade
{
    /// <summary>
    ///     Retrieves the number of domains detected today asynchronously.
    /// </summary>
    /// <returns>The number of domains detected today.</returns>
    public async Task<long> GetNumberOfDomainsTodayAsync()
    {
        DateTime startOfDay = DateTime.UtcNow.Date;
        DateTime endOfDay = startOfDay.AddDays(1);
        return await repository.CountDomainsFromStartToEndDateTimeAsync(startOfDay, endOfDay);
    }

    /// <summary>
    ///     Retrieves the number of positive detection results today asynchronously.
    /// </summary>
    /// <returns>The number of positive detection results today.</returns>
    public async Task<long> GetPositiveDetectionResultsTodayAsync()
    {
        DateTime startOfDay = DateTime.UtcNow.Date;
        DateTime endOfDay = startOfDay.AddDays(1);
        return await repository.CountPositiveResultsFromStartToEndDateTimeAsync(startOfDay, endOfDay);
    }

    /// <summary>
    ///     Retrieves the count of results filtered by blacklist asynchronously.
    /// </summary>
    /// <returns>The count of results filtered by blacklist.</returns>
    public async Task<long> GetFilteredByBlacklistCountAsync() => await repository.CountFilteredByBlacklistAsync();
}
