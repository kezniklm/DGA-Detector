using BL.Models.Blacklist;
using BL.Models.Result;
using DAL.Entities;

namespace BL.Facades.Interfaces;

public interface IBlacklistFacade : IFacade<BlacklistEntity, BlacklistModel>
{
    Task<string> MoveResultToBlacklist(ResultModel model);
}
