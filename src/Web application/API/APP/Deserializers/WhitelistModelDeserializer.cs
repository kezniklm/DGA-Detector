using APP.DTOs;
using BL.Models.Whitelist;
using MongoDB.Bson;

namespace APP.Deserializers;

public class WhitelistModelDeserializer : DeserializerBase
{
    public static WhitelistModel DeserializeWhitelistModel(WhitelistDto whitelistDto)
    {
        ObjectId id = ParseObjectId(whitelistDto.Id);

        return new WhitelistModel { Added = whitelistDto.Added, DomainName = whitelistDto.DomainName, Id = id };
    }
}
