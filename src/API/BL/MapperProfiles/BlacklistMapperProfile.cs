using AutoMapper;
using BL.Models.Blacklist;
using DAL.Entities;

namespace BL.MapperProfiles;

public class BlacklistMapperProfile : Profile
{
    public BlacklistMapperProfile()
    {
        CreateMap<BlacklistEntity, BlacklistListModel>();
        CreateMap<BlacklistEntity, BlacklistDetailModel>();

        CreateMap<BlacklistDetailModel, BlacklistEntity>();
    }
}
