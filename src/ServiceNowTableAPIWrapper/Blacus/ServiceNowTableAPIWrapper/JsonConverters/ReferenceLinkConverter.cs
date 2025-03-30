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
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    return null;
                case JsonTokenType.StartObject:
                    var objectValue = JsonSerializer.Deserialize<ReferenceLink>(ref reader);
                    return objectValue;
            }
            throw new Exception("Cannot unmarshal type LocationUnion");
        }

        public override void Write(Utf8JsonWriter writer, ReferenceLink value, JsonSerializerOptions options)
        {

            if (value.Value != null)
            {

                JsonSerializer.Serialize(writer, value.Value, options);
                return;

            }
            else
            {

                JsonSerializer.Serialize(writer, value.DisplayValue, options);
                return;

            }

            throw new Exception("Cannot marshal type LocationUnion");

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
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    return null;
                case JsonTokenType.StartObject:
                    var objectValue = JsonSerializer.Deserialize<ReferenceLink<T>>(ref reader);
                    return objectValue;
            }
            throw new Exception("Cannot unmarshal type LocationUnion");
        }

        public override void Write(Utf8JsonWriter writer, ReferenceLink<T> value, JsonSerializerOptions options)
        {

            if (value.Record != null)
            {

                JsonSerializer.Serialize(writer, value.Record.GetType().GetProperty("SysId").GetValue(value.Record), options);
                return;

            }

            if (value.Value != null)
            {

                JsonSerializer.Serialize(writer, value.Value, options);
                return;

            }
            else
            {

                JsonSerializer.Serialize(writer, value.DisplayValue, options);
                return;

            }

            throw new Exception("Cannot marshal type LocationUnion");

        }

        public static readonly ReferenceLinkConverter<T> Singleton = new();

    }

}
