using Common;

namespace DAL.Entities.Interfaces;

public interface IEntity : IWithId
{
    string DomainName { get; set; }
}
