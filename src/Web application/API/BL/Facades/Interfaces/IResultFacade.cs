using BL.Models.Result;
using DAL.Entities;

namespace BL.Facades.Interfaces;

public interface IResultFacade : IFacade<ResultEntity, ResultModel>
{
    Task<long> GetNumberOfDomainsTodayAsync();
    Task<long> GetPositiveDetectionResultsTodayAsync();
    Task<long> GetFilteredByBlacklistCountAsync();
}
