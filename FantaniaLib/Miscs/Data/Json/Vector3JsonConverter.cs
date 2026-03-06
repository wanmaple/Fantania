using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantaniaLib;

public class Vector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");
        float x = 0, y = 0, z = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Vector3(x, y, z);
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");
            string propName = reader.GetString()!;
            if (!reader.Read())
                throw new JsonException("Unexpected end of JSON");
            switch (propName)
            {
                case "X":
                    x = reader.GetSingle();
                    break;
                case "Y":
                    y = reader.GetSingle();
                    break;
                case "Z":
                    z = reader.GetSingle();
                    break;
                default:
                    throw new JsonException($"Unexpected property name: {propName}");
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteEndObject();
    }
}