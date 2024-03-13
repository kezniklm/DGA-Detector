using BL.Models.Interfaces;
using MongoDB.Bson;

namespace BL.Models.Blacklist;

public record BlacklistListModel : IModel
{
    public required string DomainName { get; set; }
    public required ObjectId Id { get; set; }
}
