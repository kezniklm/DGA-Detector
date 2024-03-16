using BL.Models.Interfaces;
using MongoDB.Bson;

namespace BL.Models.Result;

public record ResultModel : IModel
{
    public required string DomainName { get; set; }
    public DateTime Detected { get; set; }
    public bool DidBlacklistHit { get; set; }
    public double DangerousProbabilityValue { get; set; }
    public bool DangerousBoolValue { get; set; }
    public required ObjectId Id { get; set; }
}
