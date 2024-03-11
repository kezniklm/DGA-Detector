using BL.Models.Interfaces;

namespace BL.Models.Whitelist;

public record WhitelistListModel : IModel
{
    public required string DomainName { get; set; }
    public required Guid Id { get; set; }
}
