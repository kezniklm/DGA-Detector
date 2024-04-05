using DAL.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Entities;

public record ResultEntity : IEntity
{
    public DateTime Detected { get; set; }
    public bool DidBlacklistHit { get; set; }
    public double DangerousProbabilityValue { get; set; }
    public bool DangerousBoolValue { get; set; }
    public required string DomainName { get; set; }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
}
