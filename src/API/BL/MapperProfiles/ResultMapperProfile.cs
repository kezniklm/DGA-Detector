using AutoMapper;
using BL.Models.Result;
using DAL.Entities;

namespace BL.MapperProfiles;

public class ResultMapperProfile : Profile
{
    public ResultMapperProfile()
    {
        CreateMap<ResultEntity, ResultListModel>();
        CreateMap<ResultEntity, ResultDetailModel>();

        CreateMap<ResultDetailModel, ResultEntity>();
    }
}
