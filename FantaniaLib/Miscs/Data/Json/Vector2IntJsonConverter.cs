using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantaniaLib;

public class Vector2IntJsonConverter : JsonConverter<Vector2Int>
{
    public override Vector2Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");
        int x = 0, y = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Vector2Int(x, y);
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");
            string propName = reader.GetString()!;
            if (!reader.Read())
                throw new JsonException("Unexpected end of JSON");
            switch (propName)
            {
                case "X":
                    x = reader.GetInt32();
                    break;
                case "Y":
                    y = reader.GetInt32();
                    break;
                default:
                    throw new JsonException($"Unexpected property name: {propName}");
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, Vector2Int value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}