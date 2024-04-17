/**
 * @file IBlacklistFacade.cs
 *
 * @brief Defines the interface for BlacklistFacade in the business logic layer.
 *
 * This file contains the definition of the IBlacklistFacade interface, which specifies the contract for managing blacklists in the business logic layer. It extends the generic IFacade interface with specific types for BlacklistEntity and BlacklistModel.
 *
 * The main functionalities of this interface include:
 * - Inheriting CRUD operations from the IFacade interface for BlacklistEntity and BlacklistModel.
 * - Providing a method to move results to the blacklist asynchronously.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using BL.Models.Blacklist;
using BL.Models.Result;
using DAL.Entities;

namespace BL.Facades.Interfaces;

/// <summary>
///     Defines operations for managing blacklisted entities.
/// </summary>
public interface IBlacklistFacade : IFacade<BlacklistEntity, BlacklistModel>
{
    /// <summary>
    ///     Moves a result to the blacklist based on the provided model.
    /// </summary>
    /// <param name="model">The model containing information about the result to be blacklisted.</param>
    /// <returns>A task representing the asynchronous operation, returning a string indicating the outcome of the operation.</returns>
    Task<string> MoveResultToBlacklist(ResultModel model);
}
