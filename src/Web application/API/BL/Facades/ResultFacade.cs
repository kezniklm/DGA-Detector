using AutoMapper;
using BL.Facades.Interfaces;
using BL.Models.Result;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

internal class ResultFacade(IRepository<ResultEntity> repository, IMapper mapper)
    : FacadeBase<ResultListModel, ResultDetailModel, ResultEntity>(repository, mapper), IResultFacade
{
}
