using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// JSON converter converting string representations of numbers to <see cref="long"/> values.
    /// </summary>
    public class LongConverter : JsonConverter<long>
    {
        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type t) => t == typeof(long);

        /// <summary>
        /// Reads and converts JSON to <see cref="long"/>.
        /// </summary>
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (long.TryParse(value, out long result))
                {
                    return result;
                }
            }

            throw new JsonException("Cannot unmarshal type long");
        }

        /// <summary>
        /// Writes a <see cref="long"/> value to JSON as a string.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());

        /// <summary>
        /// Singleton instance of <see cref="LongConverter"/>.
        /// </summary>
        public static LongConverter Singleton { get; } = new LongConverter();
    }
}
