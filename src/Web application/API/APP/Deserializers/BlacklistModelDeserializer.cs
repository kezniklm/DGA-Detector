using APP.DTOs;
using BL.Models.Blacklist;
using MongoDB.Bson;

namespace APP.Deserializers;

public class BlacklistModelDeserializer : DeserializerBase
{
    public static BlacklistModel DeserializeBlacklistModel(BlacklistDto blacklistDto)
    {
        ObjectId id = ParseObjectId(blacklistDto.Id);

        return new BlacklistModel { Added = blacklistDto.Added, DomainName = blacklistDto.DomainName, Id = id };
    }
}
