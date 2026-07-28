using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// JSON converter converting between string values ("true"/"false") and boolean primitives.
    /// </summary>
    public class ParseBoolStringConverter : JsonConverter<bool>
    {
        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type t) => t == typeof(bool);

        /// <summary>
        /// Reads and converts JSON to <see cref="bool"/>.
        /// </summary>
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

        /// <summary>
        /// Writes a <see cref="bool"/> value to JSON as a string ("true" or "false").
        /// </summary>
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            var boolString = value ? "true" : "false";
            writer.WriteStringValue(boolString);
        }

        /// <summary>
        /// Singleton instance of <see cref="ParseBoolStringConverter"/>.
        /// </summary>
        public static readonly ParseBoolStringConverter Singleton = new ParseBoolStringConverter();
    }

    /// <summary>
    /// JSON converter converting between string values ("true"/"false") and nullable booleans.
    /// </summary>
    public class ParseNullBoolStringConverter : JsonConverter<bool?>
    {
        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type t) => t == typeof(bool?);

        /// <summary>
        /// Reads and converts JSON to nullable <see cref="bool"/>.
        /// </summary>
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

        /// <summary>
        /// Writes a nullable <see cref="bool"/> value to JSON.
        /// </summary>
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

        /// <summary>
        /// Singleton instance of <see cref="ParseNullBoolStringConverter"/>.
        /// </summary>
        public static readonly ParseNullBoolStringConverter Singleton = new();
    }
}
