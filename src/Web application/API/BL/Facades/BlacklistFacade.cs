/**
 * @file BlacklistFacade.cs
 *
 * @brief Implements the facade for managing blacklisted entities.
 *
 * This file contains the implementation of the BlacklistFacade class, which serves as a facade for managing blacklisted entities. It provides methods to move results to the blacklist and utilizes AutoMapper for mapping between domain models and entity models.
 *
 * The main functionalities of this class include:
 * - Moving results to the blacklist.
 * - Mapping between result models and blacklist models.
 * - Inheriting from FacadeBase to leverage common facade functionality.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using AutoMapper;
using BL.Facades.Interfaces;
using BL.Models.Blacklist;
using BL.Models.Result;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

/// <summary>
///     Facade for managing blacklist operations.
/// </summary>
public class BlacklistFacade(IRepository<BlacklistEntity> repository, IMapper mapper)
    : FacadeBase<BlacklistModel, BlacklistEntity>(repository, mapper), IBlacklistFacade
{
    private readonly IMapper _mapper = mapper;

    /// <summary>
    ///     Moves a result to the blacklist.
    /// </summary>
    /// <param name="model">The model representing the result to be moved.</param>
    /// <returns>The ID of the added blacklist entry.</returns>
    public async Task<string> MoveResultToBlacklist(ResultModel model)
    {
        BlacklistModel? blacklistModel = _mapper.Map<BlacklistModel>(model);

        await CreateAsync(blacklistModel);

        return blacklistModel.Id;
    }
}
