using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories;

internal class BlacklistRepository(ApiDbContext dbContext)
    : RepositoryBase<BlacklistEntity>(dbContext), IBlacklistRepository
{
}
