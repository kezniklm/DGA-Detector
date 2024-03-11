using DAL.Entities.Interfaces;

namespace DAL.Entities;

public record ResultEntity : IEntity
{
    public required string DomainName { get; set; }
    public double Value { get; set; }
    public DateTime Detected { get; set; }
    public required Guid Id { get; set; }
}
