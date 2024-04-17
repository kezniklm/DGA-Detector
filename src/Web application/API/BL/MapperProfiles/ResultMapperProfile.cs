/**
 * @file ResultMapperProfile.cs
 *
 * @brief Defines AutoMapper profiles for mapping between result entities and models.
 *
 * This file contains the implementation of the ResultMapperProfile class, which defines AutoMapper profiles for mapping between result entities and models. It utilizes the AutoMapper library for object-to-object mapping.
 *
 * The main functionalities of this file include:
 * - Mapping from ResultEntity to ResultModel.
 * - Mapping from ResultModel to ResultEntity.
 * - Mapping from ResultModel to BlacklistModel with Added property set to current date and time.
 * - Mapping from ResultModel to WhitelistModel with Added property set to current date and time.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using AutoMapper;
using BL.Models.Blacklist;
using BL.Models.Result;
using BL.Models.Whitelist;
using DAL.Entities;

namespace BL.MapperProfiles;

/// <summary>
///     Mapper profile for mapping between Result entities and models.
/// </summary>
public class ResultMapperProfile : Profile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ResultMapperProfile" /> class.
    /// </summary>
    public ResultMapperProfile()
    {
        // Map from ResultEntity to ResultModel
        CreateMap<ResultEntity, ResultModel>();

        // Map from ResultModel to ResultEntity
        CreateMap<ResultModel, ResultEntity>();

        // Map from ResultModel to BlacklistModel with Added field set to current date and time
        CreateMap<ResultModel, BlacklistModel>()
            .ForMember(dest => dest.Added, opt => opt.MapFrom(src => DateTime.Now));

        // Map from ResultModel to WhitelistModel with Added field set to current date and time
        CreateMap<ResultModel, WhitelistModel>()
            .ForMember(dest => dest.Added, opt => opt.MapFrom(src => DateTime.Now));
    }
}
