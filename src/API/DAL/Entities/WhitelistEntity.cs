using DAL.Entities.Interfaces;

namespace DAL.Entities;

public record WhitelistEntity : IEntity
{
    public required string DomainName { get; set; }
    public required DateTime Added { get; set; }
    public required Guid Id { get; set; }
}
