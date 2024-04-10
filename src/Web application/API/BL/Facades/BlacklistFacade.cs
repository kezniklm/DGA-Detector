using AutoMapper;
using BL.Facades.Interfaces;
using BL.Models.Blacklist;
using BL.Models.Result;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

internal class BlacklistFacade(IRepository<BlacklistEntity> repository, IMapper mapper)
    : FacadeBase<BlacklistModel, BlacklistEntity>(repository, mapper), IBlacklistFacade
{
    private readonly IMapper _mapper = mapper;

    public async Task<string> MoveResultToBlacklist(ResultModel model)
    {
        BlacklistModel? blacklistModel = _mapper.Map<BlacklistModel>(model);

        await CreateAsync(blacklistModel);

        return blacklistModel.Id;
    }
}
