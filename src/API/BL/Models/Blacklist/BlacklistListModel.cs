using BL.Models.Interfaces;

namespace BL.Models.Blacklist;

public record BlacklistListModel : IModel
{
    public required string DomainName { get; set; }
    public required Guid Id { get; set; }
}
