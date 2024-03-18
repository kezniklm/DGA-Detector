using System.Net;
using System.Text.Json;
using APP.DTOs;
using BL.Models.Result;
using MongoDB.Bson;

namespace APP.Deserializers;

public class ResultModelDeserializer :DeserializerBase
{
    public static ResultModel DeserializeResultModel(ResultDto resultDto)
    {
        ObjectId id = ParseObjectId(resultDto.Id);

        return new ResultModel
        {
            Id = id,
            DangerousBoolValue = resultDto.DangerousBoolValue,
            DomainName = resultDto.DomainName,
            DangerousProbabilityValue = resultDto.DangerousProbabilityValue,
            Detected = resultDto.Detected,
            DidBlacklistHit = resultDto.DidBlacklistHit
        };
    }
}
