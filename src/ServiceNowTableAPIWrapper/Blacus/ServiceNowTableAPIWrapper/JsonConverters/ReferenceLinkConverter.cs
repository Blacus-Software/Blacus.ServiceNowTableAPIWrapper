using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class ReferenceLinkConverter : JsonConverter<ReferenceLink>
    {

        public override bool CanConvert(Type t) => t == typeof(ReferenceLink);

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
                    var objectValue = JsonSerializer.Deserialize<ReferenceLink>(ref reader, options);
                    return objectValue;
            }
            throw new JsonException("Cannot unmarshal type ReferenceLink");
        }

        public override void Write(Utf8JsonWriter writer, ReferenceLink value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (value.Value != null)
            {
                JsonSerializer.Serialize(writer, value.Value, options);
            }
            else
            {
                JsonSerializer.Serialize(writer, value.DisplayValue, options);
            }
        }

        public static readonly ReferenceLinkConverter Singleton = new ReferenceLinkConverter();

    }

    public class ReferenceLinkConverter<T> : JsonConverter<ReferenceLink<T>> where T : IServiceNowTableModel
    {

        public override bool CanConvert(Type t) => t == typeof(ReferenceLink<T>);

        public override ReferenceLink<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    return null;
                case JsonTokenType.StartObject:
                    var objectValue = JsonSerializer.Deserialize<ReferenceLink<T>>(ref reader, options);
                    return objectValue;
            }
            throw new JsonException($"Cannot unmarshal type ReferenceLink<{typeof(T).Name}>");
        }

        public override void Write(Utf8JsonWriter writer, ReferenceLink<T> value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (value.Record != null)
            {
                JsonSerializer.Serialize(writer, value.Record.SysId, options);
                return;
            }

            if (value.Value != null)
            {
                JsonSerializer.Serialize(writer, value.Value, options);
            }
            else
            {
                JsonSerializer.Serialize(writer, value.DisplayValue, options);
            }
        }

        public static readonly ReferenceLinkConverter<T> Singleton = new();

    }

}
