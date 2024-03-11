using BL.Models.Interfaces;

namespace BL.Models.Result;

public record ResultListModel : IModel
{
    public required Guid Id { get; set; }
    public required string DomainName { get; set; }
}
