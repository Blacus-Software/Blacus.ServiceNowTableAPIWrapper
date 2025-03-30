using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class NullableDisplayDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    { 

        public override bool CanConvert(Type t) => t == typeof(DateTimeOffset?);

        private const string DefaultDateTimeFormat = "dd'-'MMM'-'yy' 'hh':'mm':'ss tt";

        // 25-Nov-24 08:23:02 PM

        private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
        private string? _dateTimeFormat;
        private CultureInfo? _culture;

        public DateTimeStyles DateTimeStyles
        {
            get => _dateTimeStyles;
            set => _dateTimeStyles = value;
        }

        public string? DateTimeFormat
        {
            get => _dateTimeFormat ?? string.Empty;
            set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
        }

        public CultureInfo Culture
        {
            get => _culture ?? CultureInfo.CurrentCulture;
            set => _culture = value;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            string text;


            if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                    || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
            {
                value = value.Value.ToUniversalTime();
            }

            text = value.ToString();

            writer.WriteStringValue(text);
        }

        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateText = reader.GetString();

            if (!string.IsNullOrEmpty(dateText))
            {
                var toReturn = DateTimeOffset.ParseExact(dateText, DefaultDateTimeFormat, Culture, _dateTimeStyles);
                return toReturn;
            }
            else
            {
                return null;
            }
        }


        public static readonly DisplayDateTimeOffsetConverter Singleton = new();

    }

}