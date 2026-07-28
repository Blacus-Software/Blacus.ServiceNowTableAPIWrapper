
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class ParseBoolStringConverter : JsonConverter<bool>
    {

        public override bool CanConvert(Type t) => t == typeof(bool);

        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True) return true;
            if (reader.TokenType == JsonTokenType.False) return false;

            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (bool.TryParse(value, out bool b))
                {
                    return b;
                }
            }

            throw new JsonException("Cannot unmarshal type bool");
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            var boolString = value ? "true" : "false";
            JsonSerializer.Serialize(writer, boolString, options);
            return;
        }

        public static readonly ParseBoolStringConverter Singleton = new ParseBoolStringConverter();

    }

    public class ParseNullBoolStringConverter : JsonConverter<bool?>
    {

        public override bool CanConvert(Type t) => t == typeof(bool?);

        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
            {
                return reader.GetBoolean();
            }

            if (reader.TokenType == JsonTokenType.String && bool.TryParse(reader.GetString(), out bool b))
            {
                return b;
            }

            throw new JsonException("Cannot unmarshal type bool?");

        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            var boolString = value.Value ? "true" : "false";
            writer.WriteStringValue(boolString);
        }

        public static readonly ParseNullBoolStringConverter Singleton = new();

    }

}
