/**
 * @file IResultFacade.cs
 *
 * @brief Defines the interface for the ResultFacade in the business logic layer.
 *
 * This file contains the definition of the IResultFacade interface, which specifies methods for interacting with results in the business logic layer. The ResultFacade is responsible for managing ResultEntities and ResultModels.
 *
 * The main functionalities of this interface include:
 * - Retrieving the number of domains processed today asynchronously.
 * - Retrieving the number of positive detection results today asynchronously.
 * - Retrieving the count of results filtered by the blacklist asynchronously.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 */

using BL.Models.Result;
using DAL.Entities;

namespace BL.Facades.Interfaces;

/// <summary>
///     Defines the interface for interacting with result-related operations in the business logic layer.
/// </summary>
public interface IResultFacade : IFacade<ResultEntity, ResultModel>
{
    /// <summary>
    ///     Retrieves the number of domains processed today asynchronously.
    /// </summary>
    /// <returns>The number of domains processed today.</returns>
    Task<long> GetNumberOfDomainsTodayAsync();

    /// <summary>
    ///     Retrieves the number of positive detection results today asynchronously.
    /// </summary>
    /// <returns>The number of positive detection results today.</returns>
    Task<long> GetPositiveDetectionResultsTodayAsync();

    /// <summary>
    ///     Retrieves the count of results filtered by the blacklist asynchronously.
    /// </summary>
    /// <returns>The count of results filtered by the blacklist.</returns>
    Task<long> GetFilteredByBlacklistCountAsync();
}
