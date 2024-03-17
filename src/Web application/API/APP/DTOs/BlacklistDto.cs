using System.Text.Json;

namespace APP.DTOs;

public record BlacklistDto
{
    public required string DomainName { get; set; }
    public DateTime Added { get; set; }
    public required JsonElement Id { get; set; }
}
