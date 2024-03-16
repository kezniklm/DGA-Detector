using AutoMapper;
using BL.Facades.Interfaces;
using BL.Models.Result;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

internal class ResultFacade(IResultRepository repository, IMapper mapper)
    : FacadeBase<ResultModel, ResultEntity>(repository, mapper), IResultFacade
{
    public async Task<long> GetNumberOfDomainsTodayAsync()
    {
        DateTime startOfDay = DateTime.UtcNow.Date;
        DateTime endOfDay = startOfDay.AddDays(1);
        return await repository.CountDomainsFromStartToEndDateTimeAsync(startOfDay, endOfDay);
    }

    public async Task<long> GetPositiveDetectionResultsTodayAsync()
    {
        DateTime startOfDay = DateTime.UtcNow.Date;
        DateTime endOfDay = startOfDay.AddDays(1);
        return await repository.CountPositiveResultsFromStartToEndDateTimeAsync(startOfDay, endOfDay);
    }

    public async Task<long> GetFilteredByBlacklistCountAsync() => await repository.CountFilteredByBlacklistAsync();
}
