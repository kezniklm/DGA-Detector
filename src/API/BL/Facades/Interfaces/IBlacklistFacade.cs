using BL.Models.Blacklist;
using DAL.Entities;

namespace BL.Facades.Interfaces;

public interface IBlacklistFacade : IFacade<BlacklistEntity, BlacklistListModel, BlacklistDetailModel>
{
}
