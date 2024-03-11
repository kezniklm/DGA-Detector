using BL.Models.Interfaces;

namespace BL.Models.Blacklist;

public record BlacklistDetailModel : IModel
{
    public required string DomainName { get; set; }
    public required DateTime Added { get; set; }
    public required Guid Id { get; set; }
}
