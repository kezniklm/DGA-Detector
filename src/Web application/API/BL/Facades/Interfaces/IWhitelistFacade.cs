using BL.Models.Result;
using BL.Models.Whitelist;
using DAL.Entities;

namespace BL.Facades.Interfaces;

public interface IWhitelistFacade : IFacade<WhitelistEntity, WhitelistModel>
{
    Task<string> MoveResultToWhitelist(ResultModel model);
}
