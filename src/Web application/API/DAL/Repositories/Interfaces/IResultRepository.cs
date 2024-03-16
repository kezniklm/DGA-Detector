using DAL.Entities;

namespace DAL.Repositories.Interfaces;

public interface IResultRepository : IRepository<ResultEntity>
{
    Task<long> CountFilteredByBlacklistAsync();
    Task<long> CountPositiveResultsFromStartToEndDateTimeAsync(DateTime start, DateTime end);
    Task<long> CountDomainsFromStartToEndDateTimeAsync(DateTime start, DateTime end);
}
