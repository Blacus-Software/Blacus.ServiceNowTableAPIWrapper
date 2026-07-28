using Blacus.ServiceNowTableAPIWrapper.JsonConverters;
using System;
using System.Text.Json;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Builder providing shared default <see cref="JsonSerializerOptions"/> for ServiceNow data types.
    /// </summary>
    public class JsonSerializerOptionsBuilder
    {
        private static readonly Lazy<JsonSerializerOptions> serializerOptions = new(() =>
            new(JsonSerializerDefaults.General)
            {
                Converters =
                {
                    ParseBoolStringConverter.Singleton,
                    ReferenceLinkConverter.Singleton,
                    LongConverter.Singleton,
                    UniversalDateConverter<DateTime>.Singleton,
                    UniversalDateConverter<DateTimeOffset>.Singleton,
                    UniversalDateConverter<DateOnly>.Singleton,
                },
                WriteIndented = true
            });

        /// <summary>
        /// Gets the default singleton <see cref="JsonSerializerOptions"/> configured for ServiceNow JSON data.
        /// </summary>
        public static JsonSerializerOptions Singleton => serializerOptions.Value;
    }
}
