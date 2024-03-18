using System.Text.Json;

namespace APP.DTOs;

public record ResultDto
{
    public required string DomainName { get; set; }
    public DateTime Detected { get; set; }
    public bool DidBlacklistHit { get; set; }
    public double DangerousProbabilityValue { get; set; }
    public bool DangerousBoolValue { get; set; }
    public required JsonElement Id { get; set; }
}
