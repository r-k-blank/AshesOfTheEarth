using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework; // Necesar pentru Vector2

namespace AshesOfTheEarth.Core.Serialization
{
    public class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token");
            }

            float x = 0, y = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Vector2(x, y);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected PropertyName token");
                }

                string propertyName = reader.GetString();
                reader.Read(); // Move to the value

                switch (propertyName) // Case insensitive matching for flexibility
                {
                    case "X":
                    case "x":
                        x = reader.GetSingle();
                        break;
                    case "Y":
                    case "y":
                        y = reader.GetSingle();
                        break;
                        // Ignoră alte proprietăți dacă există
                }
            }
            throw new JsonException("Expected EndObject token"); // EndObject wasn't found
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteEndObject();
        }
    }
}