using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories;

internal class ResultRepository(ApiDbContext dbContext) : RepositoryBase<ResultEntity>(dbContext), IResultRepostitory
{
}
