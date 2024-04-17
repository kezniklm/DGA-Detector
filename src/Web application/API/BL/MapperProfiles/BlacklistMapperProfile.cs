/**
 * @file BlacklistMapperProfile.cs
 *
 * @brief Defines AutoMapper profile for mapping between BlacklistEntity and BlacklistModel.
 *
 * This file contains the definition of the BlacklistMapperProfile class, which serves as an AutoMapper profile for mapping between BlacklistEntity and BlacklistModel objects. It defines mappings from BlacklistEntity to BlacklistModel and vice versa.
 *
 * @remarks
 * The main functionalities of this file include:
 * - Defining mappings from BlacklistEntity to BlacklistModel.
 * - Defining mappings from BlacklistModel to BlacklistEntity.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using AutoMapper;
using BL.Models.Blacklist;
using DAL.Entities;

namespace BL.MapperProfiles;

/// <summary>
///     Mapper profile for mapping between BlacklistEntity and BlacklistModel.
/// </summary>
public class BlacklistMapperProfile : Profile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlacklistMapperProfile" /> class.
    /// </summary>
    public BlacklistMapperProfile()
    {
        // Map from BlacklistEntity to BlacklistModel
        CreateMap<BlacklistEntity, BlacklistModel>();

        // Map from BlacklistModel to BlacklistEntity
        CreateMap<BlacklistModel, BlacklistEntity>();
    }
}
