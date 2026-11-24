namespace Blacus.ServiceNowTableAPIWrapper.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Blacus.ServiceNowTableAPIWrapper.Serialization.Converters;

/// <summary>
/// Provides preconfigured <see cref="JsonSerializerOptions"/> for ServiceNow deserialization and write serialization.
/// </summary>
public static class ServiceNowJsonOptions
{

    static ServiceNowJsonOptions()
    {

        var readOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        readOptions.Converters.Add(new DynamicRecordConverter());
        readOptions.Converters.Add(new ReferenceLinkConverterFactory());
        readOptions.Converters.Add(new ServiceNowBoolConverter());
        readOptions.Converters.Add(new ServiceNowNullableBoolConverter());
        readOptions.Converters.Add(new ServiceNowInt64Converter());
        readOptions.Converters.Add(new ServiceNowNullableInt64Converter());
        readOptions.Converters.Add(new ServiceNowDateTimeConverter());
        readOptions.Converters.Add(new ServiceNowNullableDateTimeConverter());
        readOptions.Converters.Add(new ServiceNowNullableDateTimeOffsetConverter());

        var writeResolver = new DefaultJsonTypeInfoResolver();
        writeResolver.Modifiers.Add(ReadOnlyPropertyModifier.Modify);

        var writeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = writeResolver
        };

        writeOptions.Converters.Add(new DynamicRecordConverter());
        writeOptions.Converters.Add(new ReferenceLinkConverterFactory());
        writeOptions.Converters.Add(new ServiceNowBoolConverter());
        writeOptions.Converters.Add(new ServiceNowNullableBoolConverter());
        writeOptions.Converters.Add(new ServiceNowInt64Converter());
        writeOptions.Converters.Add(new ServiceNowNullableInt64Converter());
        writeOptions.Converters.Add(new ServiceNowDateTimeConverter());
        writeOptions.Converters.Add(new ServiceNowNullableDateTimeConverter());
        writeOptions.Converters.Add(new ServiceNowNullableDateTimeOffsetConverter());

        Default = readOptions;
        Write = writeOptions;

    }

    /// <summary>
    /// Gets the default <see cref="JsonSerializerOptions"/> used for reading and deserializing ServiceNow responses.
    /// </summary>
    public static JsonSerializerOptions Default { get; }

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> configured with <see cref="ReadOnlyPropertyModifier"/> used for write requests (POST, PUT, PATCH).
    /// </summary>
    public static JsonSerializerOptions Write { get; }

}
