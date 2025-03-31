using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class DisplayDateTimeConverterX : JsonConverter<DateTime>
    { 

        public override bool CanConvert(Type t) => t == typeof(DateTime);

        private const string DefaultDateTimeFormat = "dd'-'MMM'-'yy' 'hh':'mm':'ss tt";
          
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

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {

            string text = value.ToString("yyyy-MM-dd HH:mm:ss");

            writer.WriteStringValue(text);

        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            string? dateText = reader.GetString();

            if (!string.IsNullOrEmpty(dateText))
            {
                var toReturn = DateTime.ParseExact(dateText, DefaultDateTimeFormat, Culture, _dateTimeStyles);
                return toReturn;
            }
            else
            {
                return default(DateTime);
            }
        }


        public static readonly DisplayDateTimeConverterX Singleton = new();

    }

}