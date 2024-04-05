using DAL.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Entities;

public record BlacklistEntity : IEntity
{
    public required DateTime Added { get; set; }
    public required string DomainName { get; set; }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
}
