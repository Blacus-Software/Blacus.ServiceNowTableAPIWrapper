namespace Blacus.ServiceNowTableAPIWrapper.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;
using Blacus.ServiceNowTableAPIWrapper.Models;

/// <summary>
/// JSON converter factory for <see cref="ReferenceLink"/> and <see cref="ReferenceLink{T}"/>.
/// Deserializes from either raw string sys_id or reference object {"value", "display_value", "link"},
/// and serializes outbound strictly as a raw sys_id string or null.
/// </summary>
public sealed class ReferenceLinkConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(ReferenceLink).IsAssignableFrom(typeToConvert);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(ReferenceLink))
        {
            return new ReferenceLinkConverter();
        }

        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ReferenceLink<>))
        {
            Type targetType = typeToConvert.GetGenericArguments()[0];
            Type converterType = typeof(TypedReferenceLinkConverter<>).MakeGenericType(targetType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        return new ReferenceLinkConverter();
    }

    private sealed class ReferenceLinkConverter : JsonConverter<ReferenceLink>
    {
        public override ReferenceLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadReferenceLink<ReferenceLink>(ref reader);
        }

        public override void Write(Utf8JsonWriter writer, ReferenceLink value, JsonSerializerOptions options)
        {
            WriteReferenceLink(writer, value);
        }
    }

    private sealed class TypedReferenceLinkConverter<T> : JsonConverter<ReferenceLink<T>> where T : class, IServiceNowRecord
    {
        public override ReferenceLink<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadReferenceLink<ReferenceLink<T>>(ref reader);
        }

        public override void Write(Utf8JsonWriter writer, ReferenceLink<T> value, JsonSerializerOptions options)
        {
            string? sysId = value.Value ?? value.Record?.SysId;
            if (string.IsNullOrWhiteSpace(sysId))
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(sysId);
            }
        }
    }

    internal static TLink? ReadReferenceLink<TLink>(ref Utf8JsonReader reader) where TLink : ReferenceLink, new()
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string? raw = reader.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            return new TLink { Value = raw };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var link = new TLink();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propName = reader.GetString() ?? string.Empty;
                    reader.Read();

                    if (propName.Equals("value", StringComparison.OrdinalIgnoreCase))
                    {
                        link.Value = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    }
                    else if (propName.Equals("display_value", StringComparison.OrdinalIgnoreCase))
                    {
                        link.DisplayValue = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    }
                    else if (propName.Equals("link", StringComparison.OrdinalIgnoreCase))
                    {
                        if (reader.TokenType == JsonTokenType.String &&
                            Uri.TryCreate(reader.GetString(), UriKind.Absolute, out Uri? parsedUri))
                        {
                            link.Link = parsedUri;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }

            return link;
        }

        return null;
    }

    internal static void WriteReferenceLink(Utf8JsonWriter writer, ReferenceLink? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value.Value))
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
