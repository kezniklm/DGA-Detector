using DAL.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Entities;

public record BlacklistEntity : IEntity
{
    public required string DomainName { get; set; }
    public required DateTime Added { get; set; }
    public required ObjectId Id { get; set; }
}
