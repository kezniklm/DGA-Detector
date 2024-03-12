using BL.Models.Interfaces;
using MongoDB.Bson;

namespace BL.Models.Result;

public record ResultDetailModel : IModel
{
    public required ObjectId Id { get; set; }
    public required string DomainName { get; set; }
    public DateTime Detected { get; set; }
    public double Value { get; set; }
}
