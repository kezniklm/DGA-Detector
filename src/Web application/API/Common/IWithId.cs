using MongoDB.Bson;

namespace Common;

public interface IWithId
{
    ObjectId Id { get; set; }
}
