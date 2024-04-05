using BL.Models.Interfaces;

namespace BL.Models.Blacklist;

public record BlacklistModel : IModel
{
    public required string DomainName { get; set; }
    public required DateTime Added { get; set; }
    public required string Id { get; set; }
}
