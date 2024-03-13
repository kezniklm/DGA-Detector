using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories;

internal class WhitelistRepository(ApiDbContext dbContext)
    : RepositoryBase<WhitelistEntity>(dbContext), IWhitelistRepostitory
{
}
