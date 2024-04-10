using AutoMapper;
using BL.Facades.Interfaces;
using BL.Models.Result;
using BL.Models.Whitelist;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BL.Facades;

internal class WhitelistFacade(IRepository<WhitelistEntity> repository, IMapper mapper)
    : FacadeBase<WhitelistModel, WhitelistEntity>(repository, mapper), IWhitelistFacade
{
    private readonly IMapper _mapper = mapper;
    public async Task<string> MoveResultToWhitelist(ResultModel model)
    {
        WhitelistModel? whitelistModel = _mapper.Map<WhitelistModel>(model);

        await CreateAsync(whitelistModel);

        return whitelistModel.Id;
    }
}
