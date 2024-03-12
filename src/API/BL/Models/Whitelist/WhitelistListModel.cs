using BL.Models.Interfaces;
using MongoDB.Bson;

namespace BL.Models.Whitelist;

public record WhitelistListModel : IModel
{
    public required string DomainName { get; set; }
    public required ObjectId Id { get; set; }
}
