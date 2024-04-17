/**
 * @file WhitelistFacade.cs
 *
 * @brief Provides functionality for managing whitelists.
 *
 * This file contains the implementation of the WhitelistFacade class, which is responsible for managing whitelists. It interacts with the data access layer (DAL) through the specified repository and utilizes AutoMapper for mapping between domain models and entities.
 *
 * The main functionalities of this class include:
 * - Moving results to the whitelist by mapping ResultModel to WhitelistModel.
 * - Creating entries in the whitelist asynchronously.
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
using BL.Models.Whitelist;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

/// <summary>
///     Facade for managing whitelist-related operations.
/// </summary>
internal class WhitelistFacade(IRepository<WhitelistEntity> repository, IMapper mapper)
    : FacadeBase<WhitelistModel, WhitelistEntity>(repository, mapper), IWhitelistFacade
{
    private readonly IMapper _mapper = mapper;

    /// <summary>
    ///     Moves a result to the whitelist.
    /// </summary>
    /// <param name="model">The result model to be moved to the whitelist.</param>
    /// <returns>The ID of the newly created whitelist entry.</returns>
    public async Task<string> MoveResultToWhitelist(ResultModel model)
    {
        WhitelistModel? whitelistModel = _mapper.Map<WhitelistModel>(model);

        await CreateAsync(whitelistModel);

        return whitelistModel.Id;
    }
}
