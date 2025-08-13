
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
            var value = reader.GetString();
            bool b;
            if (Boolean.TryParse(value, out b))
            {
                return b;
            }
            throw new Exception("Cannot unmarshal type bool");
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

            var value = reader;
            bool b;

            if (value.TokenType == JsonTokenType.True || value.TokenType == JsonTokenType.False)
            {
                b = value.GetBoolean();
                return b;
            }

            if (Boolean.TryParse(value.GetString(), out b))
            {
                return b;
            }

            throw new Exception("Cannot unmarshal type bool");

        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            var boolString = value.Value ? "true" : "false";
            JsonSerializer.Serialize(writer, boolString, options);
            return;
        }

        public static readonly ParseNullBoolStringConverter Singleton = new();

    }

}
