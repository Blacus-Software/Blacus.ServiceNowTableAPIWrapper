using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// JSON converter for non-generic <see cref="ReferenceLink"/> objects.
    /// </summary>
    public class ReferenceLinkConverter : JsonConverter<ReferenceLink>
    {
        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type t) => t == typeof(ReferenceLink);

        /// <summary>
        /// Reads and converts JSON into a <see cref="ReferenceLink"/>.
        /// </summary>
        public override ReferenceLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    return new ReferenceLink { Value = stringValue };
                case JsonTokenType.StartObject:
                    var objectValue = JsonSerializer.Deserialize<ReferenceLink>(ref reader, JsonSerializerOptionsBuilder.Singleton);
                    return objectValue;
            }
            throw new JsonException("Cannot unmarshal type ReferenceLink");
        }

        /// <summary>
        /// Writes a <see cref="ReferenceLink"/> object to JSON.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, ReferenceLink value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (value.Value != null)
            {
                writer.WriteStringValue(value.Value);
            }
            else if (value.DisplayValue != null)
            {
                writer.WriteStringValue(value.DisplayValue);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        /// <summary>
        /// Singleton instance of <see cref="ReferenceLinkConverter"/>.
        /// </summary>
        public static readonly ReferenceLinkConverter Singleton = new ReferenceLinkConverter();
    }

    /// <summary>
    /// JSON converter for strongly-typed <see cref="ReferenceLink{T}"/> objects.
    /// </summary>
    /// <typeparam name="T">The model type representing the referenced table.</typeparam>
    public class ReferenceLinkConverter<T> : JsonConverter<ReferenceLink<T>> where T : IServiceNowTableModel
    {
        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type t) => t == typeof(ReferenceLink<T>);

        /// <summary>
        /// Reads and converts JSON into a <see cref="ReferenceLink{T}"/>.
        /// </summary>
        public override ReferenceLink<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.String:
                    return null;
                case JsonTokenType.StartObject:
                    var objectValue = JsonSerializer.Deserialize<ReferenceLink<T>>(ref reader, JsonSerializerOptionsBuilder.Singleton);
                    return objectValue;
            }
            throw new JsonException($"Cannot unmarshal type ReferenceLink<{typeof(T).Name}>");
        }

        /// <summary>
        /// Writes a <see cref="ReferenceLink{T}"/> object to JSON.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, ReferenceLink<T> value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (value.Record != null)
            {
                if (value.Record.SysId != null)
                    writer.WriteStringValue(value.Record.SysId);
                else
                    writer.WriteNullValue();
                return;
            }

            if (value.Value != null)
            {
                writer.WriteStringValue(value.Value);
            }
            else if (value.DisplayValue != null)
            {
                writer.WriteStringValue(value.DisplayValue);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        /// <summary>
        /// Singleton instance of <see cref="ReferenceLinkConverter{T}"/>.
        /// </summary>
        public static readonly ReferenceLinkConverter<T> Singleton = new();
    }
}
