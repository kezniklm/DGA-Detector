using AutoMapper;
using BL.Facades.Interfaces;
using BL.Models.Whitelist;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

internal class WhitelistFacade(IRepository<WhitelistEntity> repository, IMapper mapper)
    : FacadeBase<WhitelistModel, WhitelistEntity>(repository, mapper), IWhitelistFacade
{
}
