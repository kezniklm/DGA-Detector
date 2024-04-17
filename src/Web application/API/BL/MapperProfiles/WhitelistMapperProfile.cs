/**
 * @file WhitelistMapperProfile.cs
 *
 * @brief Provides mapping profiles for whitelist entities and models using AutoMapper.
 *
 * This file contains the implementation of the WhitelistMapperProfile class, which defines mapping profiles for converting between WhitelistEntity and WhitelistModel using AutoMapper.
 *
 * The main functionalities of this file include:
 * - Mapping WhitelistEntity instances to WhitelistModel instances.
 * - Mapping WhitelistModel instances to WhitelistEntity instances.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using AutoMapper;
using BL.Models.Whitelist;
using DAL.Entities;

namespace BL.MapperProfiles;

/// <summary>
///     Mapper profile for mapping between WhitelistEntity and WhitelistModel.
/// </summary>
public class WhitelistMapperProfile : Profile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WhitelistMapperProfile" /> class.
    ///     Configures mapping from WhitelistEntity to WhitelistModel.
    /// </summary>
    public WhitelistMapperProfile()
    {
        // Maps from WhitelistEntity to WhitelistModel
        CreateMap<WhitelistEntity, WhitelistModel>();

        // Maps from WhitelistModel to WhitelistEntity
        CreateMap<WhitelistModel, WhitelistEntity>();
    }
}
