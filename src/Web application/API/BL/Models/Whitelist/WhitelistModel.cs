using BL.Models.Interfaces;

namespace BL.Models.Whitelist;

public record WhitelistModel : IModel
{
    public required string DomainName { get; set; }
    public required DateTime Added { get; set; }
    public required string Id { get; set; }
}
