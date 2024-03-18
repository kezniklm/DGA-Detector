using MongoDB.Bson;
using System.Net;
using System.Text.Json;

namespace APP.Deserializers;

public class DeserializerBase
{
    public static ObjectId ParseObjectId(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return ObjectId.Parse(element.GetString());
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            return ConstructObjectIdFromJson(element);
        }


        return ObjectId.Empty;
    }

    private static ObjectId ConstructObjectIdFromJson(JsonElement element)
    {
        byte[] bytes = new byte[12];

        int timestamp = GetInt32Property(element, "timestamp");
        Array.Copy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(timestamp)), 0, bytes, 0, 4);

        int machine = GetInt32Property(element, "machine");
        bytes[4] = (byte)(machine >> 16);
        bytes[5] = (byte)(machine >> 8);
        bytes[6] = (byte)machine;

        short pid = (short)GetInt32Property(element, "pid");
        bytes[7] = (byte)(pid >> 8);
        bytes[8] = (byte)pid;

        int increment = GetInt32Property(element, "increment");
        bytes[9] = (byte)(increment >> 16);
        bytes[10] = (byte)(increment >> 8);
        bytes[11] = (byte)increment;

        return new ObjectId(bytes);
    }

    private static int GetInt32Property(JsonElement element, string propertyName)
    {
        foreach (JsonProperty prop in element.EnumerateObject())
        {
            if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return prop.Value.GetInt32();
            }
        }

        throw new ArgumentException($"Property {propertyName} not found.");
    }
}
