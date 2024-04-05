using BL.Models.Interfaces;

namespace BL.Models.Result;

public record ResultModel : IModel
{
    public required string DomainName { get; set; }
    public DateTime Detected { get; set; }
    public bool DidBlacklistHit { get; set; }
    public double DangerousProbabilityValue { get; set; }
    public bool DangerousBoolValue { get; set; }
    public required string Id { get; set; }
}
