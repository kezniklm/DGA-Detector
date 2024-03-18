using System.Text.Json;

namespace APP.DTOs;

public record WhitelistDto
{
    public required string DomainName { get; set; }
    public DateTime Added { get; set; }
    public required JsonElement Id { get; set; }
}
