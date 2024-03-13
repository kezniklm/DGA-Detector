using BL.Models.Interfaces;
using MongoDB.Bson;

namespace BL.Models.Result;

public record ResultListModel : IModel
{
    public required string DomainName { get; set; }
    public required ObjectId Id { get; set; }
}
