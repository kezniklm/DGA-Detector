using BL.Models.Interfaces;

namespace BL.Models.Result;

public record ResultDetailModel : IModel
{
    public required Guid Id { get; set; }
    public required string DomainName { get; set; }
    public DateTime Detected { get; set; }
    public double Value { get; set; }
}
