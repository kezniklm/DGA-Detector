/**
 * @file IWhitelistFacade.cs
 *
 * @brief Defines the interface for the WhitelistFacade.
 *
 * This file contains the definition of the IWhitelistFacade interface, which specifies the contract for interacting with whitelists in the business logic layer (BL). It extends the IFacade interface, providing methods for working with WhitelistEntity and WhitelistModel.
 *
 * The main functionalities of this interface include:
 * - Moving a result to the whitelist.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 */

using BL.Models.Result;
using BL.Models.Whitelist;
using DAL.Entities;

namespace BL.Facades.Interfaces;

/// <summary>
///     Interface for the Whitelist Facade, which handles operations related to Whitelist entities.
/// </summary>
public interface IWhitelistFacade : IFacade<WhitelistEntity, WhitelistModel>
{
    /// <summary>
    ///     Moves a result to the whitelist.
    /// </summary>
    /// <param name="model">The ResultModel representing the result to be moved to the whitelist.</param>
    /// <returns>A string indicating the outcome of the operation.</returns>
    Task<string> MoveResultToWhitelist(ResultModel model);
}
