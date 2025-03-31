using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public class DateTimeConverter : JsonConverter<DateTime>
    {

        public override bool CanConvert(Type t) => t == typeof(DateTime);

        private List<string> formats = new()
        {
            "yyyy-MM-dd",                       // "2024-04-19"
            "yyyy-MM-dd HH':'mm':'ss",          // 
            "dd'-'MMM'-'yy' 'hh':'mm':'ss tt",  // 
            "dd-MMM-yy"                         // "19-Apr-24"
        };

        private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
        public DateTimeStyles DateTimeStyles
        {
            get => _dateTimeStyles;
            set => _dateTimeStyles = value;
        }

        private string? _dateTimeFormat;
        public string? DateTimeFormat
        {
            get => _dateTimeFormat ?? string.Empty;
            set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
        }

        private CultureInfo? _culture;
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

            DateTime toReturn = default(DateTime);
            DateTime.TryParseExact(dateText, formats.ToArray(), Culture, _dateTimeStyles, out toReturn);
            return toReturn;

        }

        public static readonly DateTimeConverter Singleton = new();

    }

}