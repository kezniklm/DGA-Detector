using AutoMapper;
using BL.Models.Blacklist;
using BL.Models.Result;
using BL.Models.Whitelist;
using DAL.Entities;

namespace BL.MapperProfiles;

public class ResultMapperProfile : Profile
{
    public ResultMapperProfile()
    {
        CreateMap<ResultEntity, ResultModel>();

        CreateMap<ResultModel, ResultEntity>();

        CreateMap<ResultModel, BlacklistModel>()
            .ForMember(dest => dest.Added, opt => opt.MapFrom(src => DateTime.Now));

        CreateMap<ResultModel, WhitelistModel>()
            .ForMember(dest => dest.Added, opt => opt.MapFrom(src => DateTime.Now));
    }
}
