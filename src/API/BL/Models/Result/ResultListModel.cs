using BL.Models.Interfaces;
using MongoDB.Bson;

namespace BL.Models.Result;

public record ResultListModel : IModel
{
    public required ObjectId Id { get; set; }
    public required string DomainName { get; set; }
}
