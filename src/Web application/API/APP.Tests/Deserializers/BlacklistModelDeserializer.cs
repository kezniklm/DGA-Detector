using System.Net;
using System.Text.Json;
using BL.Models.Blacklist;
using MongoDB.Bson;

namespace APP.Tests.Deserializers;

public static class BlacklistModelDeserializer
{
    public static BlacklistModel DeserializeBlacklistModel(string json)
    {
        JsonElement jsonObject = JsonSerializer.Deserialize<JsonElement>(json);

        DateTime added = ParseDateTime(jsonObject, "added");
        string? domainName = ParseString(jsonObject, "domainName");
        ObjectId id = ParseObjectId(jsonObject, "id");

        return new BlacklistModel { Added = added, DomainName = domainName ?? string.Empty, Id = id };
    }

    private static DateTime ParseDateTime(JsonElement jsonObject, string propertyName)
    {
        if (jsonObject.TryGetProperty(propertyName, out JsonElement element) &&
            element.ValueKind == JsonValueKind.String)
        {
            return DateTime.Parse(element.GetString() ?? string.Empty);
        }

        return DateTime.MinValue;
    }

    private static string? ParseString(JsonElement jsonObject, string propertyName)
    {
        if (jsonObject.TryGetProperty(propertyName, out JsonElement element) &&
            element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }

        return null;
    }

    private static ObjectId ParseObjectId(JsonElement jsonObject, string propertyName)
    {
        if (jsonObject.TryGetProperty(propertyName, out JsonElement element))
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return ObjectId.Parse(element.GetString());
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                return ConstructObjectIdFromJson(element);
            }
        }

        return ObjectId.Empty;
    }

    private static ObjectId ConstructObjectIdFromJson(JsonElement element)
    {
        byte[] bytes = new byte[12];

        int timestamp = element.GetProperty("timestamp").GetInt32();
        Array.Copy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(timestamp)), 0, bytes, 0, 4);

        int machine = element.GetProperty("machine").GetInt32();
        bytes[4] = (byte)(machine >> 16);
        bytes[5] = (byte)(machine >> 8);
        bytes[6] = (byte)machine;

        short pid = (short)element.GetProperty("pid").GetInt32();
        bytes[7] = (byte)(pid >> 8);
        bytes[8] = (byte)pid;

        int increment = element.GetProperty("increment").GetInt32();
        bytes[9] = (byte)(increment >> 16);
        bytes[10] = (byte)(increment >> 8);
        bytes[11] = (byte)increment;

        return new ObjectId(bytes);
    }
}
