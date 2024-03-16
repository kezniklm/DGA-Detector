using AutoMapper;
using BL.Models.Whitelist;
using DAL.Entities;

namespace BL.MapperProfiles;

public class WhitelistMapperProfile : Profile
{
    public WhitelistMapperProfile()
    {
        CreateMap<WhitelistEntity, WhitelistModel>();

        CreateMap<WhitelistModel, WhitelistEntity>();
    }
}
