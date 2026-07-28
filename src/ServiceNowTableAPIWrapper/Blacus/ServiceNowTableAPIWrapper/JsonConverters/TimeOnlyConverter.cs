using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// JSON converter for <see cref="TimeOnly"/> values.
    /// </summary>
    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private readonly string serializationFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeOnlyConverter"/> class using the default format (<c>HH:mm:ss.fff</c>).
        /// </summary>
        public TimeOnlyConverter() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeOnlyConverter"/> class with a custom formatting string.
        /// </summary>
        /// <param name="serializationFormat">The time serialization format string.</param>
        public TimeOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
        }

        /// <summary>
        /// Reads and converts JSON to <see cref="TimeOnly"/>.
        /// </summary>
        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeOnly.Parse(value!);
        }

        /// <summary>
        /// Writes a <see cref="TimeOnly"/> value to JSON.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
    }
}
