using AutoMapper;
using BL.Facades.Interfaces;
using BL.Models.Blacklist;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

internal class BlacklistFacade(IRepository<BlacklistEntity> repository, IMapper mapper)
    : FacadeBase<BlacklistModel, BlacklistEntity>(repository, mapper), IBlacklistFacade
{
}
