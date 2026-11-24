namespace Blacus.ServiceNowTableAPIWrapper.Serialization.Converters;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Deserializes JSON objects into <see cref="ExpandoObject"/>, JSON arrays into <see cref="List{Object}"/>,
/// and JSON primitives into native CLR types (<see cref="String"/>, <see cref="Boolean"/>, <see cref="Int64"/>, <see cref="Double"/>, <see langword="null"/>).
/// Prevents raw <see cref="JsonElement"/> instances from leaking into dynamic results.
/// </summary>
public sealed class DynamicRecordConverter : JsonConverter<object>
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(object) || typeToConvert == typeof(ExpandoObject);
    }

    /// <inheritdoc />
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadValue(ref reader);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    internal static object? ReadValue(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out long l))
                {
                    return l;
                }
                return reader.GetDouble();
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.StartObject:
                var expando = new ExpandoObject();
                var dict = (IDictionary<string, object?>)expando;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return expando;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propName = reader.GetString() ?? string.Empty;
                        reader.Read();
                        dict[propName] = ReadValue(ref reader);
                    }
                }
                return expando;
            case JsonTokenType.StartArray:
                var list = new List<object?>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        return list;
                    }
                    list.Add(ReadValue(ref reader));
                }
                return list;
            default:
                return null;
        }
    }
}
